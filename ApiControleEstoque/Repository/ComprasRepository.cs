using ApiControleEstoque.Models;
using ApiControleEstoque.Models.ViewModels;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiControleEstoque.Repository
{
    public class ComprasRepository
    {
        private static readonly string _connectionString;

        static ComprasRepository()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        }

        // Lista todas as compras com os nomes dos produtos e fornecedores (JOINs).
        public static async Task<List<ComprasViewModel>> GetAllComprasAsync()
        {
            var query = @"
                SELECT c.IdCompra, c.IdFornecedor, c.IdProduto, c.Data, c.Quantidade,
                       f.Nome AS FornecedorNome, p.Descricao AS ProdutoDescricao, p.CodBarras AS CodBarras
                FROM Compras c
                LEFT JOIN Fornecedores f ON c.IdFornecedor = f.IdFornecedor
                LEFT JOIN Produtos p ON c.IdProduto = p.IdProduto";
            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<ComprasViewModel>(query);
            return list.AsList();
        }

        // Busca uma compra específica por ID com dados detalhados.
        public static async Task<ComprasViewModel?> GetByIdComprasAsync(long idCompra)
        {
            if (idCompra <= 0) return null;
            var query = @"
                SELECT c.IdCompra, c.IdFornecedor, c.IdProduto, c.Data, c.Quantidade,
                       f.Nome AS FornecedorNome, p.Descricao AS ProdutoDescricao, p.CodBarras AS CodBarras
                FROM Compras c
                LEFT JOIN Fornecedores f ON c.IdFornecedor = f.IdFornecedor
                LEFT JOIN Produtos p ON c.IdProduto = p.IdProduto
                WHERE c.IdCompra = @idCompra";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<ComprasViewModel>(query, new { idCompra });
        }

        // Filtra compras de um fornecedor específico.
        public static async Task<List<Compras>> GetComprasByFornecedorAsync(long idFornecedor)
        {
            if (idFornecedor <= 0) return new List<Compras>();
            var query = "SELECT * FROM Compras WHERE IdFornecedor = @idFornecedor";
            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<Compras>(query, new { idFornecedor });
            return list.AsList();
        }

        // Filtra compras de um produto específico.
        public static async Task<List<Compras>> GetComprasByProdutoIdAsync(long idProduto)
        {
            if (idProduto <= 0) return new List<Compras>();
            var query = "SELECT * FROM Compras WHERE IdProduto = @idProduto";
            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<Compras>(query, new { idProduto });
            return list.AsList();
        }

        // Filtra compras por período de data.
        public static async Task<List<Compras>> GetComprasByPeriodoAsync(DateTime inicio, DateTime fim)
        {
            var query = "SELECT * FROM Compras WHERE Data BETWEEN @inicio AND @fim";
            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<Compras>(query, new { inicio, fim });
            return list.AsList();
        }

        // Retorna uma ficha técnica completa da compra (incluindo CNPJ e detalhes).
        public static async Task<ComprasViewModel?> GetCompraCompletaAsync(long idCompra)
        {
            if (idCompra <= 0) return null;
            var query = @"
                SELECT c.IdCompra, c.Data, c.Quantidade,
                       f.IdFornecedor, f.Nome AS FornecedorNome, f.CNPJ,
                       p.IdProduto, p.Descricao AS ProdutoDescricao, p.CodBarras AS CodBarras
                FROM Compras c
                INNER JOIN Fornecedores f ON c.IdFornecedor = f.IdFornecedor
                INNER JOIN Produtos p ON c.IdProduto = p.IdProduto
                WHERE c.IdCompra = @idCompra";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<ComprasViewModel>(query, new { idCompra });
        }

        // Pesquisa compras usando filtros cruzados enviados via JSON (POST).
        public static async Task<List<ComprasViewModel>> ConsultarPorTudoAsync(FiltroParaCompra filtro)
        {
            var query = @"
                SELECT c.*, f.Nome AS FornecedorNome, p.Descricao AS ProdutoDescricao, p.CodBarras
                FROM Compras c
                INNER JOIN Fornecedores f ON c.IdFornecedor = f.IdFornecedor
                INNER JOIN Produtos p ON c.IdProduto = p.IdProduto
                WHERE (@DataInicio IS NULL OR c.Data >= @DataInicio)
                  AND (@DataFim IS NULL OR c.Data <= @DataFim)
                  AND (@IdProduto IS NULL OR c.IdProduto = @IdProduto)
                  AND (@IdFornecedor IS NULL OR c.IdFornecedor = @IdFornecedor)
                  AND (@QtdeMin IS NULL OR c.Quantidade >= @QtdeMin)";
            
            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<ComprasViewModel>(query, new { 
                DataInicio = filtro.DataInicio, 
                DataFim = filtro.DataFim,
                IdProduto = filtro.IdProduto,
                IdFornecedor = filtro.IdFornecedor,
                QtdeMin = filtro.QuantidadeMin
            });
            return list.AsList();
        }

        // OPERAÇÃO MESTRE: Registra a Compra e automaticamente dá entrada no estoque físico.
        public static async Task<int> ComprarAsync(SolicitacaoCompra solicitacao)
        {
            if (solicitacao.Quantidade <= 0) return -1;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Cadastra a Compra
                var queryCompra = @"
                    INSERT INTO Compras (IdFornecedor, IdProduto, Data, Quantidade)
                    VALUES (@IdFornecedor, @IdProduto, @Data, @Quantidade);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var idCompra = await connection.QuerySingleAsync<int>(queryCompra, solicitacao, transaction);

                // 2. Automático: Registrar Movimentação de Estoque de Entrada (ID 1, Tipo 1)
                var queryMov = @"
                    INSERT INTO MovimentacoesEstoque 
                    (IdEstoque, IdTipoMovimentacaoEstoque, IdFuncionarioSolicitador, IdProduto, Quantidade, DataHora)
                    VALUES (1, 1, @IdFuncionario, @IdProduto, @Quantidade, @Data)";

                await connection.ExecuteAsync(queryMov, solicitacao, transaction);

                transaction.Commit();
                return idCompra;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Cadastra uma compra simples (apenas registro, sem afetar estoque físico).
        public static async Task<int> CreateCompraAsync(Compras compra)
        {
            if (compra.Quantidade <= 0) return -1;

            var query = @"
                INSERT INTO Compras (IdFornecedor, IdProduto, Data, Quantidade)
                VALUES (@IdFornecedor, @IdProduto, @Data, @Quantidade)";

            using var connection = new SqlConnection(_connectionString);
            try
            {
                return await connection.ExecuteAsync(query, compra);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) return -2;
                throw;
            }
        }

        // Atualiza os dados de uma compra já existente.
        public static async Task<int> UpdateComprasAsync(Compras compra)
        {
            if (compra.IdCompra <= 0 || compra.Quantidade <= 0) return -1;

            var query = @"
                UPDATE Compras
                SET IdFornecedor = @IdFornecedor,
                    IdProduto = @IdProduto,
                    Data = @Data,
                    Quantidade = @Quantidade
                WHERE IdCompra = @IdCompra";

            using var connection = new SqlConnection(_connectionString);
            try
            {
                return await connection.ExecuteAsync(query, compra);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) return -2;
                throw;
            }
        }

        public static async Task<int> DeleteComprasAsync(long idCompra)
        {
            if (idCompra <= 0) return 0;
            var query = "DELETE FROM Compras WHERE IdCompra = @idCompra";
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(query, new { idCompra });
        }
    }
}
