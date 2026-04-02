using ApiControleEstoque.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiControleEstoque.Repository
{
    public class FornecedoresRepository
    {
        private static readonly string _connectionString;

        static FornecedoresRepository()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        }

        public static async Task<List<Fornecedores>> GetAllAsync()
        {
            var query = "SELECT IdFornecedor, CNPJ, Nome FROM Fornecedores";
            using var connection = new SqlConnection(_connectionString);
            var listFornecedores = await connection.QueryAsync<Fornecedores>(query);
            return listFornecedores.AsList();
        }

        public static async Task<Fornecedores?> GetByIdFornecedorAsync(long idFornecedor)
        {
            var query = "SELECT IdFornecedor, CNPJ, Nome FROM Fornecedores WHERE IdFornecedor = @IdFornecedor";
            using var connection = new SqlConnection(_connectionString);
            var fornecedor = await connection.QueryFirstOrDefaultAsync<Fornecedores>(query, new { IdFornecedor = idFornecedor });
            return fornecedor;
        }

        public static async Task<Fornecedores?> GetByCNPJAsync(string cnpj)
        {
            var query = "SELECT IdFornecedor, CNPJ, Nome FROM Fornecedores WHERE CNPJ = @CNPJ";
            using var connection = new SqlConnection(_connectionString);
            var fornecedor = await connection.QueryFirstOrDefaultAsync<Fornecedores>(query, new { CNPJ = cnpj });
            return fornecedor;
        }

        public static async Task<bool> ExistsFornecedoresByCNPJAsync(string cnpj, long? excludeId = null)
        {
            var query = "SELECT COUNT(1) FROM Fornecedores WHERE CNPJ = @CNPJ";

            if (excludeId != null)
            {
                query += " AND IdFornecedor != @IdFornecedor";
            }

            using var connection = new SqlConnection(_connectionString);
            var count = await connection.ExecuteScalarAsync<int>(query, new { CNPJ = cnpj, IdFornecedor = excludeId });
            return count > 0;
        }

        public static async Task<int> AddAsync(Fornecedores fornecedor)
        {
            if (string.IsNullOrWhiteSpace(fornecedor.CNPJ) || string.IsNullOrWhiteSpace(fornecedor.Nome))
                return -1; // Dados inválidos

            var existsFornecedores = await ExistsFornecedoresByCNPJAsync(fornecedor.CNPJ);
            if (existsFornecedores)
                return 0;

            var query = @"
                INSERT INTO Fornecedores (IdFornecedor, CNPJ, Nome)
                VALUES (@IdFornecedor, @CNPJ, @Nome)";

            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(query, fornecedor);
            return rowsAffected;
        }


        public static async Task<int> UpdateFornecedorAsync(Fornecedores fornecedor)
        {
            if (string.IsNullOrWhiteSpace(fornecedor.CNPJ) || string.IsNullOrWhiteSpace(fornecedor.Nome))
                return -1; // Dados inválidos

            var existsFornecedores = await ExistsFornecedoresByCNPJAsync(fornecedor.CNPJ, fornecedor.IdFornecedor);
            if (existsFornecedores)
                return 0;

            var query = @"
                UPDATE Fornecedores
                SET CNPJ = @CNPJ,
                    Nome = @Nome
                WHERE IdFornecedor = @IdFornecedor";

            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(query, fornecedor);
            return rowsAffected;
        }


        public static async Task<int> DeleteFornecedoresByIdAsync(long idFornecedor)
        {
            var query = "DELETE FROM Fornecedores WHERE IdFornecedor = @IdFornecedor";
            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(query, new { IdFornecedor = idFornecedor });
            return rowsAffected;
        }
    }
}
