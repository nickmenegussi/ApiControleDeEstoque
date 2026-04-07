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
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnection");
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
            if (idFornecedor <= 0) return null;
            var query = "SELECT IdFornecedor, CNPJ, Nome FROM Fornecedores WHERE IdFornecedor = @IdFornecedor";
            using var connection = new SqlConnection(_connectionString);
            var fornecedor = await connection.QueryFirstOrDefaultAsync<Fornecedores>(query, new { IdFornecedor = idFornecedor });
            return fornecedor;
        }

        public static async Task<Fornecedores?> GetByCNPJAsync(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj)) return null;
            var query = "SELECT IdFornecedor, CNPJ, Nome FROM Fornecedores WHERE CNPJ = @CNPJ";
            using var connection = new SqlConnection(_connectionString);
            var fornecedor = await connection.QueryFirstOrDefaultAsync<Fornecedores>(query, new { CNPJ = cnpj });
            return fornecedor;
        }

        public static async Task<List<Fornecedores>> ConsultarPorNomeAsync(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome)) return new List<Fornecedores>();
            var query = "SELECT IdFornecedor, CNPJ, Nome FROM Fornecedores WHERE Nome LIKE @Nome";
            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<Fornecedores>(query, new { Nome = $"%{nome}%" });
            return list.AsList();
        }

        public static async Task<bool> ExistsFornecedoresByCNPJAsync(string cnpj, long? excludeId = null)
        {
            var query = "SELECT COUNT(1) FROM Fornecedores WHERE CNPJ = @CNPJ";

            if (excludeId != null)
                query += " AND IdFornecedor != @IdFornecedor";

            using var connection = new SqlConnection(_connectionString);
            var count = await connection.ExecuteScalarAsync<int>(query, new { CNPJ = cnpj, IdFornecedor = excludeId });
            return count > 0;
        }

        public static async Task<int> AddAsync(Fornecedores fornecedor)
        {
            if (string.IsNullOrWhiteSpace(fornecedor.CNPJ) || string.IsNullOrWhiteSpace(fornecedor.Nome))
                return -1;

            var existsFornecedores = await ExistsFornecedoresByCNPJAsync(fornecedor.CNPJ);
            if (existsFornecedores) return 0;

            var query = @"
                INSERT INTO Fornecedores (CNPJ, Nome)
                VALUES (@CNPJ, @Nome)";

            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(query, new {
                fornecedor.CNPJ,
                fornecedor.Nome
            });
            return rowsAffected;
        }

        public static async Task<int> UpdateFornecedorAsync(Fornecedores fornecedor)
        {
            if (fornecedor.IdFornecedor <= 0) return 0;
            if (string.IsNullOrWhiteSpace(fornecedor.CNPJ) || string.IsNullOrWhiteSpace(fornecedor.Nome))
                return -1;

            var existsFornecedores = await ExistsFornecedoresByCNPJAsync(fornecedor.CNPJ, fornecedor.IdFornecedor);
            if (existsFornecedores) return 0;

            var query = @"
                UPDATE Fornecedores
                SET CNPJ = @CNPJ,
                    Nome = @Nome
                WHERE IdFornecedor = @IdFornecedor";

            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(query, new {
                fornecedor.CNPJ,
                fornecedor.Nome,
                fornecedor.IdFornecedor
            });
            return rowsAffected;
        }

        public static async Task<int> DeleteFornecedoresByIdAsync(long IdFornecedor)
        {
            if (IdFornecedor <= 0) return 0;
            var query = "DELETE FROM Fornecedores WHERE IdFornecedor = @IdFornecedor";
            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(query, new { IdFornecedor });
            return rowsAffected;
        }
    }
}
