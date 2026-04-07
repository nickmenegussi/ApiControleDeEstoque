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
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnection");
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
            if (idTipoEstoque <= 0) return null;
            var query = "SELECT IdTipoEstoque, Descricao FROM TiposEstoque WHERE IdTipoEstoque = @IdTipoEstoque";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<TiposEstoque>(query, new { IdTipoEstoque = idTipoEstoque });
        }

        public static async Task<List<TiposEstoque>?> GetAllTiposEstoqueByDescricaoAsync(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao)) return new List<TiposEstoque>();
            var query = "SELECT IdTipoEstoque, Descricao FROM TiposEstoque WHERE Descricao LIKE @Descricao";
            using var connection = new SqlConnection(_connectionString);
            var listTiposEstoque = await connection.QueryAsync<TiposEstoque>(query, new { Descricao = $"%{descricao}%" });
            return listTiposEstoque.AsList();
        }

        public static async Task<bool> ExistsDescricaoTipoEstoqueAsync(string Descricao, long? idAtual = null)
        {
            var query = "SELECT COUNT(1) FROM TiposEstoque WHERE Descricao = @Descricao";

            if (idAtual != null)
                query += " AND IdTipoEstoque != @IdTipoEstoque";

            using var connection = new SqlConnection(_connectionString);
            var count = await connection.ExecuteScalarAsync<int>(query, new { Descricao, IdTipoEstoque = idAtual });
            return count > 0;
        }

        public static async Task<int> CreateTiposEstoqueAsync(TiposEstoque tiposEstoque)
        {
            if (string.IsNullOrWhiteSpace(tiposEstoque.Descricao)) return -1;
            var existsDescricao = await ExistsDescricaoTipoEstoqueAsync(tiposEstoque.Descricao);
            if (existsDescricao) return 0;

            var query = @"
                INSERT INTO TiposEstoque (Descricao)       
                VALUES(@Descricao)";
            using var connection = new SqlConnection(_connectionString);
            var affectedRows = await connection.ExecuteAsync(query, new { tiposEstoque.Descricao });
            return affectedRows;
        }

        public static async Task<int> UpdateTiposEstoqueAsync(TiposEstoque tipo)
        {
            if (tipo.IdTipoEstoque <= 0) return 0;
            if (string.IsNullOrWhiteSpace(tipo.Descricao)) return -1;
            var existsDescricao = await ExistsDescricaoTipoEstoqueAsync(tipo.Descricao, tipo.IdTipoEstoque);
            if (existsDescricao) return 0;

            var query = @"
                UPDATE TiposEstoque
                SET Descricao = @Descricao
                WHERE IdTipoEstoque = @IdTipoEstoque";

            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(query, new { tipo.Descricao, tipo.IdTipoEstoque });
        }

        public static async Task<int> DeleteTiposEstoqueByIdAsync(long IdTipoEstoque)
        {
            if (IdTipoEstoque <= 0) return 0;
            string query = "DELETE FROM TiposEstoque WHERE IdTipoEstoque = @IdTipoEstoque";
            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(query, new { IdTipoEstoque });
            return rowsAffected;
        }
    }
}
