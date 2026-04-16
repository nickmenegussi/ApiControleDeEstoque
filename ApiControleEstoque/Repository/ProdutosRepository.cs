using ApiControleEstoque.Models;
using ApiControleEstoque.Models.ViewModels;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiControleEstoque.Repository
{
    public class ProdutosRepository
    {
        private static readonly string _connectionString;

        static ProdutosRepository()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        }

        public static async Task<List<Produtos>> GetAllProdutosAsync()
        {
            var query = "SELECT IdProduto, CodBarras, Descricao FROM Produtos";
            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<Produtos>(query);
            return list.AsList();
        }

        public static async Task<Produtos?> GetByIdAsync(long idProduto)
        {
            if (idProduto <= 0) return null;
            var query = "SELECT IdProduto, CodBarras, Descricao FROM Produtos WHERE IdProduto = @idProduto";
            using var connection = new SqlConnection(_connectionString);
            var produto = await connection.QueryFirstOrDefaultAsync<Produtos>(query, new { idProduto });
            return produto;
        }

        public static async Task<bool> ExistsCodBarraAsync(string CodBarras, long? idAtual = null)
        {
            var query = "SELECT COUNT(1) FROM Produtos WHERE CodBarras = @CodBarras";
            if (idAtual != null) query += " AND IdProduto != @idAtual";

            using var connection = new SqlConnection(_connectionString);
            var count = await connection.ExecuteScalarAsync<int>(query, new { CodBarras, idAtual });
            return count > 0;
        }

        public static async Task<int> CreateProdutoAsync(Produtos produto)
        {
            if (string.IsNullOrWhiteSpace(produto.CodBarras) || string.IsNullOrWhiteSpace(produto.Descricao))
                return -1; // Dados inválidos

            var existsCodBarra = await ExistsCodBarraAsync(produto.CodBarras);
            if (existsCodBarra) return 0; // Já existe

            var query = "INSERT INTO Produtos (CodBarras, Descricao) VALUES (@CodBarras, @Descricao)";
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(query, new { produto.CodBarras, produto.Descricao});
        }

        public static async Task<int> UpdateProdutoAsync(Produtos produto)
        {
            if (produto.IdProduto <= 0) return 0;
            if (string.IsNullOrWhiteSpace(produto.CodBarras) || string.IsNullOrWhiteSpace(produto.Descricao))
                return -1; // Dados inválidos

            var existsCodBarra = await ExistsCodBarraAsync(produto.CodBarras, produto.IdProduto);
            if (existsCodBarra) return 0; // Já existe

            var query = "UPDATE Produtos SET Descricao = @Descricao, CodBarras = @CodBarras WHERE IdProduto = @IdProduto";
            using var connection = new SqlConnection(_connectionString);
            var affectedRows = await connection.ExecuteAsync(query, produto);
            return affectedRows;
        }

        public static async Task<List<Produtos>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<Produtos>();
            var sql = @"
                SELECT IdProduto, CodBarras, Descricao FROM Produtos 
                WHERE Descricao LIKE @Query OR CodBarras LIKE @Query";

            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<Produtos>(sql, new { Query = $"%{query}%" });
            return list.AsList();
        }

        // Calcula em tempo real o saldo de um produto em cada depósito da empresa.
        public static async Task<List<ProdutoEstoqueQuantidadeViewModel>> ListarQuantidadePorEstoqueAsync(long idProduto)
        {
            if (idProduto <= 0) return new List<ProdutoEstoqueQuantidadeViewModel>();
            /* 
               Lógica de Cálculo de Saldo (Balance):
               - Tipos 1 e 4: Entradas de estoque (Sinal Positivo +)
               - Tipos 2 e 3: Saídas de estoque (Sinal Negativo -)
            */
            var query = @"
                SELECT e.IdEstoque, e.Descricao AS EstoqueNome, 
                       SUM(CASE WHEN m.IdTipoMovimentacaoEstoque IN (1, 4) THEN m.Quantidade ELSE 0 END) - 
                       SUM(CASE WHEN m.IdTipoMovimentacaoEstoque IN (2, 3) THEN m.Quantidade ELSE 0 END) AS QuantidadeFinal
                FROM MovimentacoesEstoque m
                INNER JOIN Estoque e ON m.IdEstoque = e.IdEstoque
                WHERE m.IdProduto = @IdProduto
                GROUP BY e.IdEstoque, e.Descricao
                HAVING (SUM(CASE WHEN m.IdTipoMovimentacaoEstoque IN (1, 4) THEN m.Quantidade ELSE 0 END) - 
                        SUM(CASE WHEN m.IdTipoMovimentacaoEstoque IN (2, 3) THEN m.Quantidade ELSE 0 END)) > 0";

            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<ProdutoEstoqueQuantidadeViewModel>(query, new { IdProduto = idProduto });
            return list.AsList();
        }

        // Lista o histórico recente de quem mexeu no produto e onde.
        public static async Task<List<ProdutoMovimentacaoRecenteViewModel>> ListarMovimentacoesRecentesAsync(long idProduto)
        {
            if (idProduto <= 0) return new List<ProdutoMovimentacaoRecenteViewModel>();
            var query = @"
            SELECT TOP 10 m.IdMovimentacaoEstoque, m.Quantidade, m.DataHora, 
                   tm.Descricao AS TipoMovimentacao, e.Descricao AS EstoqueNome,
                   fs.Nome AS SolicitadorNome, fa.Nome AS AutenticadorNome
            FROM MovimentacoesEstoque m
            INNER JOIN TiposMovimentacaoEstoque tm ON m.IdTipoMovimentacaoEstoque = tm.IdTipoMovimentacaoEstoque
            INNER JOIN Estoque e ON m.IdEstoque = e.IdEstoque
            LEFT JOIN Funcionarios fs ON m.IdFuncionarioSolicitador = fs.IdFuncionario
            LEFT JOIN Funcionarios fa ON m.IdFuncionarioAutenticador = fa.IdFuncionario
            WHERE m.IdProduto = @IdProduto
            ORDER BY m.DataHora DESC";

            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<ProdutoMovimentacaoRecenteViewModel>(query, new { IdProduto = idProduto });
            return list.AsList();
        }

        // Pesquisa centralizada em ID, Descrição ou Código de Barras (POST).
        public static async Task<List<Produtos>> SearchByFilterAsync(long? id = null, string? codBarras = null, string? descricao = null)
        {
            var codBarraLimpo = string.IsNullOrWhiteSpace(codBarras) ? null : codBarras;
            var descricaoLimpa = string.IsNullOrWhiteSpace(descricao) ? null : descricao;

            string query = @"
                SELECT IdProduto, CodBarras, Descricao FROM Produtos 
                WHERE (@Id IS NULL OR IdProduto = @Id)
                  AND (@CodBarra IS NULL OR CodBarras = @CodBarra)
                  AND (@Descricao IS NULL OR Descricao LIKE '%' + @Descricao + '%')";

            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<Produtos>(query, new {
                Id = id,
                CodBarra = codBarraLimpo,
                Descricao = descricaoLimpa
            });
            return list.AsList();
        }

        public static async Task<int> DeleteProdutoAsync(long idProduto)
        {
            if (idProduto <= 0) return 0;
            var query = "DELETE FROM Produtos WHERE IdProduto = @idProduto";
            using var connection = new SqlConnection(_connectionString);
            var affectedRows = await connection.ExecuteAsync(query, new { idProduto });
            return affectedRows;
        }
    }
}
