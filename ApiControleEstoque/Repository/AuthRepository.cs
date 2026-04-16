using ApiControleEstoque.Models;
using Dapper;
using Microsoft.Data.SqlClient;
// using BCrypt.Net; // Ativar quando for usar criptografia real

namespace ApiControleEstoque.Repository
{
    public class AuthRepository
    {
        private static readonly string _connectionString;

        static AuthRepository()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        }

        // Realiza o Login comparando a senha digitada.
        public static async Task<Funcionarios?> AuthenticateFuncionarioAsync( string email, string senha)
        {
            if (string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(email)) return null;

            using var connection = new SqlConnection(_connectionString);
            var funcionario = await FuncionariosRepository.GetByEmailAsync(email, true);

            if (funcionario == null) return null;

            // SEGURANÇA (BCRYPT) 
            // if (!BCrypt.Net.BCrypt.Verify(senha, funcionario.Senha)) return null;
            
            if (senha != funcionario.Senha) return null;

            // --- TOKEN JWT ---
            // Local ideal para gerar o Token JWT e devolver para o Controller.
            // Ex: var token = JwtService.GenerateToken(funcionario);

            // Por segurança, limpamos a senha do objeto antes de enviar para o Frontend.
            funcionario.Senha = string.Empty;
            return funcionario;
        }

        // Atualiza a senha.
        public static async Task<bool> UpdatePasswordAsync(long idFuncionario, string senhaAtual, string novaSenha) 
        {
            if (idFuncionario <= 0) return false;
            if (string.IsNullOrWhiteSpace(senhaAtual) || string.IsNullOrWhiteSpace(novaSenha)) return false;

            using var connection = new SqlConnection(_connectionString);
            var funcionario = await FuncionariosRepository.GetByIdFuncionarioAsync(idFuncionario);

            if (funcionario == null) return false;

            // VERIFICAÇÃO DA SENHA ATUAL
            
            // if (!BCrypt.Net.BCrypt.Verify(senhaAtual, funcionario.Senha)) return false;
            if (senhaAtual != funcionario.Senha) return false;

            // CRIPTOGRAFIA DA NOVA SENHA 
            // var novaSenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
            var novaSenhaSalvar = novaSenha;

            var query = @"
               UPDATE Funcionarios 
               SET Senha = @novaSenhaSalvar
               WHERE IdFuncionario = @IdFuncionario";

            var rowsAffected = await connection.ExecuteAsync(query, new { novaSenhaSalvar, IdFuncionario = idFuncionario });
            return rowsAffected > 0;
        }
    }

}
