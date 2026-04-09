using ApiControleEstoque.Models;
using ApiControleEstoque.Models.ViewModels;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiControleEstoque.Repository
{
    public class EstoqueRepository
    {
        private static readonly string _connectionString;

        static EstoqueRepository()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public static async Task<List<EstoquesViewModel>> GetAllEstoqueAsync()
        {
            var query = @"
            SELECT 
                e.IdEstoque,
                e.IdTipoEstoque,
                e.Descricao,
                te.Descricao AS DescricaoTiposEstoque
            FROM Estoque e
            LEFT JOIN TiposEstoque te ON e.IdTipoEstoque = te.IdTipoEstoque";
            using var connection = new SqlConnection(_connectionString);
            var listEstoque = await connection.QueryAsync<EstoquesViewModel>(query);
            return listEstoque.AsList();
        }

        public static async Task<EstoquesViewModel?> GetByIdAsync(long id)
        {
            if (id <= 0) return null;
            var query = @"
            SELECT 
                e.IdEstoque, e.IdTipoEstoque, e.Descricao,
                te.Descricao AS DescricaoTiposEstoque
            FROM Estoque e
            LEFT JOIN TiposEstoque te ON e.IdTipoEstoque = te.IdTipoEstoque
            WHERE e.IdEstoque = @id";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<EstoquesViewModel>(query, new { id });
        }

        public static async Task<List<EstoquesViewModel>> GetEstoqueByTipoEstoqueAsync(string Descricao)
        {
            if (string.IsNullOrWhiteSpace(Descricao)) return new List<EstoquesViewModel>();
            var query = @"
            SELECT 
                e.IdEstoque, e.IdTipoEstoque, e.Descricao,
                te.Descricao AS DescricaoTiposEstoque
            FROM Estoque e     
            INNER JOIN TiposEstoque te ON e.IdTipoEstoque = te.IdTipoEstoque
            WHERE te.Descricao LIKE @Desc";
            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<EstoquesViewModel>(query, new { Desc = $"%{Descricao}%" });
            return list.AsList();
        }

        public static async Task<List<EstoquesViewModel>> GetEstoqueByIdTipoEstoqueAsync(long idTipoEstoque)
        {
            if (idTipoEstoque <= 0) return new List<EstoquesViewModel>();
            var query = @"
            SELECT 
                e.IdEstoque, e.IdTipoEstoque, e.Descricao,
                te.Descricao AS DescricaoTiposEstoque
            FROM Estoque e
            LEFT JOIN TiposEstoque te ON e.IdTipoEstoque = te.IdTipoEstoque
            WHERE e.IdTipoEstoque = @IdTipoEstoque";
            using var connection = new SqlConnection(_connectionString);
            return (await connection.QueryAsync<EstoquesViewModel>(query, new { IdTipoEstoque = idTipoEstoque })).AsList();
        }

        public static async Task<bool> ExistsDescricaoNoTipoEstoqueAync(string Descricao, long? IdTipoEstoque = null, long? IdEstoque = null)
        {
            var query = "SELECT COUNT(1) FROM Estoque WHERE Descricao = @Descricao AND IdTipoEstoque = @IdTipoEstoque";

            if (IdEstoque != null)
                query += " AND IdEstoque != @IdEstoque";

            using var connection = new SqlConnection(_connectionString);
            var count = await connection.ExecuteScalarAsync<int>(query, new { Descricao, IdTipoEstoque, IdEstoque });
            return count > 0;
        }

        public static async Task<int> AddEstoqueAsync(Estoque estoque)
        {
            if (string.IsNullOrWhiteSpace(estoque.Descricao)) return -1;
            var existsDescricaoNoTipoEstoque = await ExistsDescricaoNoTipoEstoqueAync(estoque.Descricao, estoque.IdTipoEstoque);

            if (existsDescricaoNoTipoEstoque) return 0;

            var query = @"
            INSERT INTO Estoque (IdTipoEstoque, Descricao) 
            VALUES (@IdTipoEstoque, @Descricao)";

            using var connection = new SqlConnection(_connectionString);
            var affectedRows = await connection.ExecuteAsync(query, new { estoque.IdTipoEstoque, estoque.Descricao });
            return affectedRows;
        }

        public static async Task<int> UpdateEstoqueAsync(Estoque estoque)
        {
            if (estoque.IdEstoque <= 0) return 0;
            if (string.IsNullOrWhiteSpace(estoque.Descricao)) return -1;
            var existsDescricaoNoTipoEstoque = await ExistsDescricaoNoTipoEstoqueAync(estoque.Descricao, estoque.IdTipoEstoque, estoque.IdEstoque);

            if (existsDescricaoNoTipoEstoque) return 0;

            var query = @"
            UPDATE Estoque
            SET IdTipoEstoque = @IdTipoEstoque,
                Descricao = @Descricao
            WHERE IdEstoque = @IdEstoque";
            using var connection = new SqlConnection(_connectionString);
            var affectedRows = await connection.ExecuteAsync(query, new { estoque.IdTipoEstoque, estoque.Descricao, estoque.IdEstoque });
            return affectedRows;
        }

        public static async Task<List<EstoquesViewModel>> GetEstoqueByDescricaoAsync(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao)) return new List<EstoquesViewModel>();
            var query = @"
            SELECT 
                e.IdEstoque, e.IdTipoEstoque, e.Descricao,
                te.Descricao AS DescricaoTiposEstoque
            FROM Estoque e
            LEFT JOIN TiposEstoque te ON e.IdTipoEstoque = te.IdTipoEstoque
            WHERE e.Descricao LIKE @Desc";
            using var connection = new SqlConnection(_connectionString);
            return (await connection.QueryAsync<EstoquesViewModel>(query, new { Desc = $"%{descricao}%" })).AsList();
        }

        public static async Task<object> GetQuantidadeProdutosNoEstoqueAsync(long idEstoque)
        {
            if (idEstoque <= 0) return new List<dynamic>();
            var query = @"
            SELECT p.IdProduto, p.Descricao, p.CodBarra,
                   SUM(CASE WHEN m.IdTipoMovimentacaoEstoque IN (1, 4) THEN m.Quantidade ELSE 0 END) - 
                   SUM(CASE WHEN m.IdTipoMovimentacaoEstoque IN (2, 3) THEN m.Quantidade ELSE 0 END) AS QuantidadeFinal
            FROM MovimentacoesEstoque m
            INNER JOIN Produtos p ON m.IdProduto = p.IdProduto
            WHERE m.IdEstoque = @IdEstoque
            GROUP BY p.IdProduto, p.Descricao, p.CodBarra
            HAVING (SUM(CASE WHEN m.IdTipoMovimentacaoEstoque IN (1, 4) THEN m.Quantidade ELSE 0 END) - 
                    SUM(CASE WHEN m.IdTipoMovimentacaoEstoque IN (2, 3) THEN m.Quantidade ELSE 0 END)) > 0";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<dynamic>(query, new { IdEstoque = idEstoque });
        }

        public static async Task<object> GetMovimentacoesRecentesAsync(long idEstoque)
        {
            if (idEstoque <= 0) return new List<dynamic>();
            var query = @"
            SELECT TOP 10 m.IdMovimentacaoEstoque, m.Quantidade, m.DataHora, 
                   tm.Descricao AS TipoMovimentacao, p.Descricao AS Produto
            FROM MovimentacoesEstoque m
            INNER JOIN TiposMovimentacaoEstoque tm ON m.IdTipoMovimentacaoEstoque = tm.IdTipoMovimentacaoEstoque
            INNER JOIN Produtos p ON m.IdProduto = p.IdProduto
            WHERE m.IdEstoque = @IdEstoque
            ORDER BY m.DataHora DESC";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<dynamic>(query, new { IdEstoque = idEstoque });
        }

        public static async Task<object> GetQuantidadeProdutoEmTodosEstoquesAsync(string codBarra)
        {
            if (string.IsNullOrWhiteSpace(codBarra)) return new List<dynamic>();
            var query = @"
            SELECT e.IdEstoque, e.Descricao AS EstoqueNome, p.CodBarra, p.Descricao AS Produto,
                   SUM(CASE WHEN m.IdTipoMovimentacaoEstoque IN (1, 4) THEN m.Quantidade ELSE 0 END) - 
                   SUM(CASE WHEN m.IdTipoMovimentacaoEstoque IN (2, 3) THEN m.Quantidade ELSE 0 END) AS QuantidadeFinal
            FROM MovimentacoesEstoque m
            INNER JOIN Produtos p ON m.IdProduto = p.IdProduto
            INNER JOIN Estoque e ON m.IdEstoque = e.IdEstoque
            WHERE p.CodBarra = @CodBarra
            GROUP BY e.IdEstoque, e.Descricao, p.CodBarra, p.Descricao
            HAVING (SUM(CASE WHEN m.IdTipoMovimentacaoEstoque IN (1, 4) THEN m.Quantidade ELSE 0 END) - 
                    SUM(CASE WHEN m.IdTipoMovimentacaoEstoque IN (2, 3) THEN m.Quantidade ELSE 0 END)) > 0";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<dynamic>(query, new { CodBarra = codBarra });
        }

        public static async Task<List<EstoquesViewModel>> ConsultarIdTipoEstoqueEstoqueTipoEstoqueAsync(PesquisaEstoque vm)
        {
            if (vm == null || string.IsNullOrWhiteSpace(vm.Descricao) || vm.IdTipoEstoque <= 0) 
                return new List<EstoquesViewModel>();
            var query = @"
            SELECT 
                e.IdEstoque, e.IdTipoEstoque, e.Descricao AS Descricao,
                te.Descricao AS DescricaoTiposEstoque
            FROM Estoque e
            INNER JOIN TiposEstoque te ON e.IdTipoEstoque = te.IdTipoEstoque
            WHERE e.IdTipoEstoque = @IdTipoEstoque AND e.Descricao LIKE @Desc";

            using var connection = new SqlConnection(_connectionString);
            return (await connection.QueryAsync<EstoquesViewModel>(query, new { IdTipoEstoque = vm.IdTipoEstoque, Desc = $"%{vm.Descricao}%" })).AsList();
        }

        
    }
}