using ApiControleEstoque.Models;
using ApiControleEstoque.Models.ViewModels;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiControleEstoque.Repository
{
    public class FuncionariosRepository
    {
        private static readonly string _connectionString;

        static FuncionariosRepository()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public static async Task<List<Funcionarios>> GetAllFuncionariosAsync(bool? showPassword = false)
        {
            var columns = "IdFuncionario, Nome, Email, Setor";
            if (showPassword == true) columns += ", Senha";

            var query = $"SELECT {columns} FROM Funcionarios";
            using var connection = new SqlConnection(_connectionString);
            var funcionarios = await connection.QueryAsync<Funcionarios>(query);
            return funcionarios.AsList();
        }

        public static async Task<Funcionarios?> GetByIdFuncionarioAsync(long idFuncionario)
        {
            if (idFuncionario <= 0) return null;
            string query = "SELECT * FROM Funcionarios WHERE IdFuncionario = @IdFuncionario";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Funcionarios>(query, new { IdFuncionario = idFuncionario });
        }

        public static async Task<Funcionarios?> GetByEmailAsync(string email, bool includePassword = false)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var columns = "IdFuncionario, Nome, Email, Setor";
            if (includePassword) columns += ", Senha";

            string query = $"SELECT {columns} FROM Funcionarios WHERE Email = @Email";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Funcionarios>(query, new { Email = email });
        }

        // Redundant search methods removed. Use SearchByFilterAsync below.

        public static async Task<List<FuncionarioMovimentacaoViewModel>> ListarMovimentacoesDoFuncionarioAsync(long idFuncionario)
        {
            if (idFuncionario <= 0) return new List<FuncionarioMovimentacaoViewModel>();
            string query = @"
                SELECT m.IdMovimentacaoEstoque, m.Quantidade, m.DataHora, 
                       p.Descricao AS Produto, e.Descricao AS Estoque, tm.Descricao AS TipoMovimentacao
                FROM MovimentacoesEstoque m
                INNER JOIN Produtos p ON m.IdProduto = p.IdProduto
                INNER JOIN Estoque e ON m.IdEstoque = e.IdEstoque
                INNER JOIN TiposMovimentacaoEstoque tm ON m.IdTipoMovimentacaoEstoque = tm.IdTipoMovimentacaoEstoque
                WHERE m.IdFuncionarioSolicitador = @IdFuncionario 
                   OR m.IdFuncionarioAutenticador = @IdFuncionario
                ORDER BY m.DataHora DESC";

            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<FuncionarioMovimentacaoViewModel>(query, new { IdFuncionario = idFuncionario });
            return list.AsList();
        }

        public static async Task<List<Funcionarios>> SearchByFilterAsync(long? id = null, string? nome = null, string? setor = null, string? email = null)
        {
            var nomeLimpo = string.IsNullOrWhiteSpace(nome) ? null : nome;
            var setorLimpo = string.IsNullOrWhiteSpace(setor) ? null : setor;
            var emailLimpo = string.IsNullOrWhiteSpace(email) ? null : email;

            string query = @"
                SELECT IdFuncionario, Nome, Email, Setor FROM Funcionarios 
                WHERE (@Id IS NULL OR IdFuncionario = @Id)
                  AND (@Nome IS NULL OR Nome LIKE '%' + @Nome + '%')
                  AND (@Setor IS NULL OR Setor LIKE '%' + @Setor + '%')
                  AND (@Email IS NULL OR Email = @Email)";

            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<Funcionarios>(query, new {
                Id = id,
                Nome = nomeLimpo,
                Setor = setorLimpo,
                Email = emailLimpo
            });
            return list.AsList();
        }

        public static async Task<bool> ExistsByEmailAsync(string email)
        {
            string query = "SELECT COUNT(1) FROM Funcionarios WHERE Email = @Email";
            using var connection = new SqlConnection(_connectionString);
            var count = await connection.ExecuteScalarAsync<int>(query, new { Email = email });
            return count > 0;
        }

        public static async Task<int> CreateFuncionarioAsync(Funcionarios funcionario)
        {
            if (string.IsNullOrWhiteSpace(funcionario.Nome) ||
                string.IsNullOrWhiteSpace(funcionario.Email) ||
                string.IsNullOrWhiteSpace(funcionario.Senha)) return -1;

            if (await ExistsByEmailAsync(funcionario.Email)) return 0;

            string query = @"
                INSERT INTO Funcionarios (Nome, Email, Setor, Senha)
                VALUES (@Nome, @Email, @Setor, @Senha)";

            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(query, funcionario);
        }

        public static async Task<int> UpdateFuncionarioAsync(Funcionarios funcionario)
        {
            if (funcionario.IdFuncionario <= 0) return 0;
            if (string.IsNullOrWhiteSpace(funcionario.Nome) ||
                string.IsNullOrWhiteSpace(funcionario.Email)) return -1;

            string query = @"
                UPDATE Funcionarios
                SET Nome = @Nome,
                    Email = @Email,
                    Setor = @Setor
                WHERE IdFuncionario = @IdFuncionario";
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(query, funcionario);
        }

        public static async Task<int> DeleteFuncionarioByIdAsync(long idFuncionario)
        {
            if (idFuncionario <= 0) return 0;
            string query = "DELETE FROM Funcionarios WHERE IdFuncionario = @IdFuncionario";
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(query, new { IdFuncionario = idFuncionario });
        }
    }
}
