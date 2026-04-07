using ApiControleEstoque.Models;
using ApiControleEstoque.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ApiControleEstoque.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // Rota de Login: Recebe Nome ou Email + Senha
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var funcionario = await AuthRepository.AuthenticateFuncionarioAsync(request.Email, request.Senha);

                if (funcionario == null)
                    return Unauthorized(new { message = "Usuário ou Senha inválidos." });

                // --- GERAÇÃO DE JWT ---
                // Caso queira usar Tokens JWT no futuro, o token seria devolvido aqui.
                // var token = JwtService.GenerateToken(funcionario);

                return Ok(new
                {
                    message = "Login realizado com sucesso!",
                    funcionario = funcionario,
                    // token = token // Exemplo de retorno de token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao processar login", error = ex.Message });
            }
        }

        // Rota para alteração de senha segura
        [HttpPost("alterar-senha")]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaRequest request)
        {
            try
            {
                if (request.IdFuncionario <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var sucesso = await AuthRepository.UpdatePasswordAsync(request.IdFuncionario, request.SenhaAtual, request.NovaSenha);

                if (!sucesso)
                    return BadRequest(new { message = "Não foi possível alterar a senha. Verifique se a senha atual está correta." });

                return Ok(new { message = "Senha alterada com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao alterar senha", error = ex.Message });
            }
        }
    }
}
