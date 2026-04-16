using ApiControleEstoque.Models;
using ApiControleEstoque.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ApiControleEstoque.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TiposMovimentacaoEstoqueController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var list = await TiposMovimentacaoEstoqueRepository.GetAllTiposMovimentacaoEstoquesAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar tipos de movimentação", error = ex.Message });
            }
        }

        [HttpGet("{idTipoMovimentacaoEstoque}")]
        public async Task<IActionResult> ListarMovimentacoesPorTipo(long idTipoMovimentacaoEstoque)
        {
            try
            {
                if (idTipoMovimentacaoEstoque <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var list = await TiposMovimentacaoEstoqueRepository.GetMovimentacoesPorTipoAsync(idTipoMovimentacaoEstoque);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar movimentações por tipo", error = ex.Message });
            }
        }

        [HttpPost("descricao")]
        public async Task<IActionResult> ConsultarPorDescricao([FromBody] PesquisaPadrao pesquisa)
        {
            try
            {
                var list = await TiposMovimentacaoEstoqueRepository.ConsultarPorDescricaoAsync(pesquisa.Query);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar tipos de movimentação por descrição", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TiposMovimentacaoEstoque tipo)
        {
            try
            {
                var result = await TiposMovimentacaoEstoqueRepository.CreateTiposMovimentacaoEstoquesAsync(tipo);
                if (result == 0) return BadRequest(new { message = "Tipo de movimentação já cadastrado." });
                if (result == -1) return BadRequest(new { message = "Descrição é obrigatória." });
                return Created("", new { message = "Tipo de movimentação cadastrado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao cadastrar tipo de movimentação", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] TiposMovimentacaoEstoque tipo)
        {
            try
            {
                if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });
                tipo.IdTipoMovimentacaoEstoque = id;
                var result = await TiposMovimentacaoEstoqueRepository.UpdateTiposMovimentacaoEstoquesAsync(tipo);
                if (result == 0) return BadRequest(new { message = "Tipo de movimentação já cadastrado para outro registro." });
                if (result == -1) return BadRequest(new { message = "Descrição é obrigatória." });
                return Ok(new { message = "Tipo de movimentação atualizado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao atualizar tipo de movimentação", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

            try
            {
                var result = await TiposMovimentacaoEstoqueRepository.DeleteTiposMovimentacaoEstoquesAsync(id);
                if (result == 0) return NotFound(new { message = "Tipo de movimentação não encontrado." });
                return Ok(new { message = "Tipo de movimentação excluído com sucesso." });
            }
            catch (Exception ex)
            {
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 547 || ex is Microsoft.Data.SqlClient.SqlException seq && seq.Number == 547)
                {
                    return BadRequest(new { message = "Não é possível excluir o tipo de movimentação pois ele já está em uso em movimentações de estoque." });
                }
                return StatusCode(500, new { message = "Erro ao excluir tipo de movimentação", error = ex.Message });
            }
        }
    }
}
