using ApiControleEstoque.Models;
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

        public static async Task<Funcionarios?> GetByEmailOrNomeFuncionarioAsync(string email, string nome)
        {
            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(nome)) return null;
            string query = "SELECT * FROM Funcionarios WHERE Email = @Email OR Nome = @Nome";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Funcionarios>(query, new { Email = email, Nome = nome });
        }

        public static async Task<Funcionarios?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            string query = "SELECT IdFuncionario, Nome, Email, Setor FROM Funcionarios WHERE Email = @Email";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Funcionarios>(query, new { Email = email });
        }

        public static async Task<List<Funcionarios>> ConsultarPorNomeAsync(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome)) return new List<Funcionarios>();
            string query = "SELECT IdFuncionario, Nome, Email, Setor FROM Funcionarios WHERE Nome LIKE @Nome";
            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<Funcionarios>(query, new { Nome = $"%{nome}%" });
            return list.AsList();
        }

        public static async Task<List<Funcionarios>> ConsultarPorSetorAsync(string setor)
        {
            if (string.IsNullOrWhiteSpace(setor)) return new List<Funcionarios>();
            string query = "SELECT IdFuncionario, Nome, Email, Setor FROM Funcionarios WHERE Setor LIKE @Setor";
            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<Funcionarios>(query, new { Setor = $"%{setor}%" });
            return list.AsList();
        }

        public static async Task<object> ListarMovimentacoesDoFuncionarioAsync(long idFuncionario)
        {
            if (idFuncionario <= 0) return new List<dynamic>();
            string query = @"
                SELECT m.IdMovimentacaoEstoque, m.Quantidade, m.DataHora, 
                       p.Descricao AS Produto, e.Descricao AS Estoque, tm.Descricao AS TipoMovimentacao
                FROM MovimentacoesEstoque m
                INNER JOIN Produtos p ON m.IdProduto = p.IdProduto
                INNER JOIN Estoques e ON m.IdEstoque = e.IdEstoque
                INNER JOIN TiposMovimentacaoEstoque tm ON m.IdTipoMovimentacaoEstoque = tm.IdTipoMovimentacaoEstoque
                WHERE m.IdFuncionarioSolicitador = @IdFuncionario 
                   OR m.IdFuncionarioAutenticador = @IdFuncionario
                ORDER BY m.DataHora DESC";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<dynamic>(query, new { IdFuncionario = idFuncionario });
        }

        public static async Task<List<Funcionarios>> ConsultarPorFiltroAsync(long? id, string? nome, string? setor, string? email)
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
