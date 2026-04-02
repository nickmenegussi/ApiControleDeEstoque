using ApiControleEstoque.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiControleEstoque.Repository
{
    public class TiposEstoqueRepository
    {
        private static readonly string _connectionString;

        static TiposEstoqueRepository()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        }

        public static async Task<List<TiposEstoque>> GetAllTiposEstoqueAsync()
        {
            var query = "SELECT IdTipoEstoque, Descricao FROM TiposEstoque";
            using var connection = new SqlConnection(_connectionString);
            var listTiposEstoque = await connection.QueryAsync<TiposEstoque>(query);
            return listTiposEstoque.AsList();
        }

        public static async Task<TiposEstoque?> GetByIdAsync(long idTipoEstoque)
        {
            var query = "SELECT IdTipoEstoque, Descricao FROM TiposEstoque WHERE IdTipoEstoque = @IdTipoEstoque";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<TiposEstoque>(query, new { IdTipoEstoque = idTipoEstoque });
        }

        public static async Task<List<TiposEstoque>> GetAllTiposEstoqueByDescricaoAsync(string descricao)
        {
            var query = "SELECT IdTipoEstoque, Descricao FROM TiposEstoque WHERE Descricao LIKE @Descricao";
            using var connection = new SqlConnection(_connectionString);
            var listTiposEstoque = await connection.QueryAsync<TiposEstoque>(query, new { Descricao = $"%{descricao}%" });
            return listTiposEstoque.AsList();
        }

        public static async Task<bool> ExistsDescricaoTipoEstoqueAsync(string descricao, long? idAtual = null)
        {
            var query = "SELECT COUNT(1) FROM TiposEstoque WHERE Descricao = @Descricao";

            if (idAtual != null)
            {
                query += " AND IdTipoEstoque != @IdTipoEstoque";
            }

            using var connection = new SqlConnection(_connectionString);
            var count = await connection.ExecuteScalarAsync<int>(query, new { Descricao = descricao, IdTipoEstoque = idAtual });
            return count > 0;
        }

        public static async Task<int> CreateTiposEstoqueAsync(TiposEstoque tiposEstoque)
        {
            var existsDescricao = await ExistsDescricaoTipoEstoqueAsync(tiposEstoque.Descricao ?? "");
            if (existsDescricao) return 0;

            if (string.IsNullOrWhiteSpace(tiposEstoque.Descricao)) return -1;

            var query = @"
                INSERT INTO TiposEstoque (IdTipoEstoque, Descricao)       
                VALUES(@IdTipoEstoque, @Descricao)
                ";
            using var connection = new SqlConnection(_connectionString);
            var affectedRows = await connection.ExecuteAsync(query, tiposEstoque);
            return affectedRows;
        }


        public static async Task<int> UpdateTiposEstoqueAsync(TiposEstoque tipo)
        {
            var existsDescricao = await ExistsDescricaoTipoEstoqueAsync(tipo.Descricao ?? "", tipo.IdTipoEstoque);
            if (existsDescricao) return 0;

            if (string.IsNullOrWhiteSpace(tipo.Descricao))
                return -1;

            var query = @"
                UPDATE TiposEstoque
                SET Descricao = @Descricao
                WHERE IdTipoEstoque = @IdTipoEstoque";

            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(query, tipo);
        }
        
        public static async Task<int> DeleteTiposEstoqueByIdAsync(long idTipoEstoque)
        {
            string query = "DELETE FROM TiposEstoque WHERE IdTipoEstoque = @IdTipoEstoque";
            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(query, new { IdTipoEstoque = idTipoEstoque });
            return rowsAffected;
        }
    }
}
