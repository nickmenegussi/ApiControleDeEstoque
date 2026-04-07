using ApiControleEstoque.Models;
using ApiControleEstoque.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ApiControleEstoque.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FuncionariosController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] bool? showPassword)
        {
            try
            {
                var list = await FuncionariosRepository.GetAllFuncionariosAsync(showPassword);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar funcionários", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var item = await FuncionariosRepository.GetByIdFuncionarioAsync(id);
                if (item == null) return NotFound(new { message = "Funcionário não encontrado." });
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar funcionário", error = ex.Message });
            }
        }

        [HttpGet("nome/{nome}")]
        public async Task<IActionResult> GetByNome(string nome)
        {
            try
            {
                var list = await FuncionariosRepository.ConsultarPorNomeAsync(nome);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar funcionários por nome", error = ex.Message });
            }
        }

        [HttpGet("setor/{setor}")]
        public async Task<IActionResult> GetBySetor(string setor)
        {
            try
            {
                var list = await FuncionariosRepository.ConsultarPorSetorAsync(setor);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar funcionários por setor", error = ex.Message });
            }
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            try
            {
                var item = await FuncionariosRepository.GetByEmailAsync(email);
                if (item == null) return NotFound(new { message = "Funcionário não encontrado." });
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar funcionário por e-mail", error = ex.Message });
            }
        }

        [HttpGet("movimentacoesFuncionario/{idFuncionario}")]
        public async Task<IActionResult> GetMovimentacoes(long idFuncionario)
        {
            try
            {
                if (idFuncionario <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var result = await FuncionariosRepository.ListarMovimentacoesDoFuncionarioAsync(idFuncionario);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar movimentações do funcionário", error = ex.Message });
            }
        }

        [HttpPost("IdNomeSetorEmail")]
        public async Task<IActionResult> PostIdNomeSetorEmail([FromBody] PesquisaFuncionario pesquisa)
        {
            try
            {
                var list = await FuncionariosRepository.ConsultarPorFiltroAsync(pesquisa.Id, pesquisa.Nome, pesquisa.Setor, pesquisa.Email);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao filtrar funcionários", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Funcionarios funcionario)
        {
            try
            {
                var result = await FuncionariosRepository.CreateFuncionarioAsync(funcionario);
                if (result == 0) return BadRequest(new { message = "E-mail já cadastrado." });
                if (result == -1) return BadRequest(new { message = "Dados obrigatórios (Nome, E-mail e Senha) não foram informados." });
                return Created("", new { message = "Funcionário cadastrado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao cadastrar funcionário", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] Funcionarios funcionario)
        {
            try
            {
                if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                funcionario.IdFuncionario = id;
                var result = await FuncionariosRepository.UpdateFuncionarioAsync(funcionario);
                if (result == 0) return NotFound(new { message = "Funcionário não encontrado." });
                if (result == -1) return BadRequest(new { message = "Dados obrigatórios (Nome e E-mail) não foram informados." });
                return Ok(new { message = "Funcionário atualizado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao atualizar funcionário", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

            try
            {
                var result = await FuncionariosRepository.DeleteFuncionarioByIdAsync(id);
                if (result == 0) return NotFound(new { message = "Funcionário não encontrado." });
                return Ok(new { message = "Funcionário excluído com sucesso." });
            }
            catch (Exception ex)
            {
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 547 || ex is Microsoft.Data.SqlClient.SqlException seq && seq.Number == 547)
                {
                    return BadRequest(new { message = "Não é possível excluir o funcionário pois ele possui movimentações registradas." });
                }
                return StatusCode(500, new { message = "Erro ao excluir funcionário", error = ex.Message });
            }
        }
    }
}
