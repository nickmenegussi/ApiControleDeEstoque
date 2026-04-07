using ApiControleEstoque.Models;
using ApiControleEstoque.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ApiControleEstoque.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TiposEstoqueController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var result = await TiposEstoqueRepository.GetAllTiposEstoqueAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar tipos de estoque", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });
                var result = await TiposEstoqueRepository.GetByIdAsync(id);
                if (result == null) return NotFound(new { message = "Tipo de estoque não encontrado." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar tipo de estoque", error = ex.Message });
            }
        }

        [HttpGet("descricao/{descricao}")]
        public async Task<IActionResult> GetByDescricao(string descricao)
        {
            try
            {
                var result = await TiposEstoqueRepository.GetAllTiposEstoqueByDescricaoAsync(descricao);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar tipo de estoque por descrição", error = ex.Message });
            }
        }

        [HttpGet("TiposEstoques/{idTipoEstoque}")]
        public async Task<IActionResult> ListarEstoquesPorTipo(long idTipoEstoque)
        {
            try
            {
                if (idTipoEstoque <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var list = await EstoquesRepository.GetEstoquesByIdTipoEstoqueAsync(idTipoEstoque);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar estoques por tipo", error = ex.Message });
            }
        }

        [HttpPost("descricao")]
        public async Task<IActionResult> ConsultarPorDescricao([FromBody] PesquisaPadrao pesquisa)
        {
            try
            {
                var list = await TiposEstoqueRepository.GetAllTiposEstoqueByDescricaoAsync(pesquisa.Query);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar tipos de estoque por descrição", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TiposEstoque tipo)
        {
            try
            {
                var affected = await TiposEstoqueRepository.CreateTiposEstoqueAsync(tipo);
                if (affected == 0) return BadRequest(new { message = "Descrição já cadastrada." });
                if (affected == -1) return BadRequest(new { message = "Descrição é obrigatória." });
                return Created("", new { message = "Tipo criado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao cadastrar tipo de estoque", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] TiposEstoque tipo)
        {
            try
            {
                if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });
                tipo.IdTipoEstoque = id;
                var affected = await TiposEstoqueRepository.UpdateTiposEstoqueAsync(tipo);
                if (affected == 0) return BadRequest(new { message = "Descrição já cadastrada para outro tipo." });
                if (affected == -1) return BadRequest(new { message = "Descrição é obrigatória." });
                return Ok(new { message = "Tipo atualizado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao atualizar tipo de estoque", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

            try
            {
                var affected = await TiposEstoqueRepository.DeleteTiposEstoqueByIdAsync(id);
                if (affected == 0) return NotFound(new { message = "Tipo de estoque não encontrado." });
                return Ok(new { message = "Tipo excluído com sucesso" });
            }
            catch (Exception ex)
            {
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 547 || ex is Microsoft.Data.SqlClient.SqlException seq && seq.Number == 547)
                {
                    return BadRequest(new { message = "Não é possível excluir o tipo de estoque pois ele já possui vínculo em estoques." });
                }
                return StatusCode(500, new { message = "Erro ao excluir tipo de estoque", error = ex.Message });
            }
        }
    }
}
