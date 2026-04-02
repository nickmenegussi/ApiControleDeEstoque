using ApiControleEstoque.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiControleEstoque.Repository
{
    public class ProdutosRepository
    {
        private static readonly string _connectionString;

        static ProdutosRepository()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        }

        public static async Task<List<Produto>> GetAllProdutosAsync()
        {
            var query = "SELECT IdProduto, CodBarras, Descricao FROM Produtos";
            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<Produto>(query);
            return list.AsList();
        }

        public static async Task<Produto?> GetByIdAsync(long idProduto)
        {
            var query = "SELECT IdProduto, CodBarras, Descricao FROM Produtos WHERE IdProduto = @idProduto";
            using var connection = new SqlConnection(_connectionString);
            var produto = await connection.QueryFirstOrDefaultAsync<Produto>(query, new { idProduto });
            return produto;
        }

        public static async Task<bool> ExistsCodBarrasAsync(string codBarras, long? idAtual = null)
        {
            var query = "SELECT COUNT(1) FROM Produtos WHERE CodBarras = @codBarras";
            if (idAtual != null) query += " AND IdProduto != @idAtual";

            using var connection = new SqlConnection(_connectionString);
            var count = await connection.ExecuteScalarAsync<int>(query, new { codBarras, idAtual });
            return count > 0;
        }

        public static async Task<int> CreateProdutoAsync(Produto produto)
        {
            if (string.IsNullOrWhiteSpace(produto.CodBarras) || string.IsNullOrWhiteSpace(produto.Descricao))
                return -1; // Dados inválidos

            var existsCodBarras = await ExistsCodBarrasAsync(produto.CodBarras);

            if (existsCodBarras) return 0; // Já existe

            var query = "INSERT INTO Produtos (IdProduto, CodBarras, Descricao) VALUES (@IdProduto, @CodBarras, @Descricao)";
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(query, produto);
        }

        public static async Task<int> UpdateProdutoAsync(Produto produto)
        {
            if (string.IsNullOrWhiteSpace(produto.CodBarras) || string.IsNullOrWhiteSpace(produto.Descricao))
                return -1; // Dados inválidos

            var existsCodBarras = await ExistsCodBarrasAsync(produto.CodBarras, produto.IdProduto);

            if (existsCodBarras) return 0; // Já existe

            var query = "UPDATE Produtos SET Descricao = @Descricao, CodBarras = @CodBarras WHERE IdProduto = @IdProduto";
            using var connection = new SqlConnection(_connectionString);
            var affectedRows = await connection.ExecuteAsync(query, produto);
            return affectedRows;
        }

        public static async Task<int> DeleteProdutoAsync(long idProduto)
        {
            // Se houver compras vinculadas, o SQL vai barrar por causa da Foreign Key
            var query = "DELETE FROM Produtos WHERE IdProduto = @idProduto";
            using var connection = new SqlConnection(_connectionString);
            var affectedRows = await connection.ExecuteAsync(query, new { idProduto });
            return affectedRows;
        }
    }
}
