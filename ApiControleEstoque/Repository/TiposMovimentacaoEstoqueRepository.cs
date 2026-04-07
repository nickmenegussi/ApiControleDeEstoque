using ApiControleEstoque.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiControleEstoque.Repository
{
    public class TiposMovimentacaoEstoqueRepository
    {
        private static readonly string _connectionString;

        static TiposMovimentacaoEstoqueRepository()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public static async Task<List<TiposMovimentacaoEstoque>> GetAllTiposMovimentacaoEstoquesAsync()
        {
            var query = "SELECT IdTipoMovimentacaoEstoque, Descricao FROM TiposMovimentacaoEstoque";
            using var connection = new SqlConnection(_connectionString);
            var listTiposMovimentacaoEstoque = await connection.QueryAsync<TiposMovimentacaoEstoque>(query);
            return listTiposMovimentacaoEstoque.AsList();
        }

        public static async Task<List<MovimentacoesEstoque>> GetMovimentacoesPorTipoAsync(long idTipoMovimentacaoEstoque)
        {
            if (idTipoMovimentacaoEstoque <= 0) return new List<MovimentacoesEstoque>();
            var query = "SELECT * FROM MovimentacoesEstoque WHERE IdTipoMovimentacaoEstoque = @idTipoMovimentacaoEstoque";
            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<MovimentacoesEstoque>(query, new { idTipoMovimentacaoEstoque });
            return list.AsList();
        }

        public static async Task<List<TiposMovimentacaoEstoque>> ConsultarPorDescricaoAsync(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao)) return new List<TiposMovimentacaoEstoque>();
            var query = "SELECT * FROM TiposMovimentacaoEstoque WHERE Descricao LIKE @Desc";
            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<TiposMovimentacaoEstoque>(query, new { Desc = $"%{descricao}%" });
            return list.AsList();
        }

        public static async Task<bool> ExistsDescricaoAsync(string descricao, long? idAtual = null)
        {
            var query = "SELECT COUNT(1) FROM TiposMovimentacaoEstoque WHERE Descricao = @descricao";

            if (idAtual != null)
                query += " AND IdTipoMovimentacaoEstoque != @idAtual";

            using var connection = new SqlConnection(_connectionString);
            var count = await connection.ExecuteScalarAsync<int>(query, new { Descricao = descricao, idAtual });
            return count > 0;
        }

        public static async Task<int> CreateTiposMovimentacaoEstoquesAsync(TiposMovimentacaoEstoque tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo.Descricao)) return -1;
            var existsDescricao = await ExistsDescricaoAsync(tipo.Descricao);
            if (existsDescricao) return 0;

            var query = "INSERT INTO TiposMovimentacaoEstoque (Descricao) VALUES (@Descricao)";
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(query, new { tipo.Descricao });
        }

        public static async Task<int> UpdateTiposMovimentacaoEstoquesAsync(TiposMovimentacaoEstoque tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo.Descricao)) return -1;
            var existsDescricao = await ExistsDescricaoAsync(tipo.Descricao, tipo.IdTipoMovimentacaoEstoque);
            if (existsDescricao) return 0;

            var query = "UPDATE TiposMovimentacaoEstoque SET Descricao = @Descricao WHERE IdTipoMovimentacaoEstoque = @IdTipoMovimentacaoEstoque";
            using var connection = new SqlConnection(_connectionString);
            var affectedRows = await connection.ExecuteAsync(query, new { tipo.Descricao, tipo.IdTipoMovimentacaoEstoque });
            return affectedRows;
        }

        public static async Task<int> DeleteTiposMovimentacaoEstoquesAsync(long IdTipoMovimentacaoEstoque)
        {
            if (IdTipoMovimentacaoEstoque <= 0) return 0;
            var query = "DELETE FROM TiposMovimentacaoEstoque WHERE IdTipoMovimentacaoEstoque = @IdTipoMovimentacaoEstoque";
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(query, new { IdTipoMovimentacaoEstoque });
        }
    }
}
