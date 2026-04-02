using ApiControleEstoque.Models;
using ApiControleEstoque.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ApiControleEstoque.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComprasController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var result = await ComprasRepository.GetAllComprasAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar compras", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                var result = await ComprasRepository.GetByIdComprasAsync(id);
                if (result == null) return NotFound("Compra não encontrada.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar compra", error = ex.Message });
            }
        }

        [HttpGet("fornecedor/{idFornecedor}")]
        public async Task<IActionResult> GetByFornecedor(long idFornecedor)
        {
            try
            {
                var result = await ComprasRepository.GetComprasByFornecedorAsync(idFornecedor);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar compras do fornecedor", error = ex.Message });
            }
        }

        [HttpGet("produto/{idProduto}")]
        public async Task<IActionResult> GetByProduto(long idProduto)
        {
            try
            {
                var result = await ComprasRepository.GetComprasByProdutoIdAsync(idProduto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar compras do produto", error = ex.Message });
            }
        }

        [HttpGet("periodo")]
        public async Task<IActionResult> GetByPeriodo([FromQuery] DateTime inicio, [FromQuery] DateTime fim)
        {
            try
            {
                var result = await ComprasRepository.GetComprasByPeriodoAsync(inicio, fim);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar compras por período", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Compras compra)
        {
            try
            {
                var affected = await ComprasRepository.CreateCompraAsync(compra);
                if (affected == -1) return BadRequest(new { message = "A quantidade deve ser maior que zero." });
                if (affected == -2) return BadRequest(new { message = "Produto ou Fornecedor informado não existe no sistema." });
                if (affected == 0) return BadRequest(new { message = "Não foi possível cadastrar a compra." });
                
                return Created("", new { message = "Compra cadastrada com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao cadastrar compra", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] Compras compra)
        {
            try
            {
                compra.IdCompra = id;
                var affected = await ComprasRepository.UpdateComprasAsync(compra);
                if (affected == -1) return BadRequest(new { message = "A quantidade deve ser maior que zero." });
                if (affected == -2) return BadRequest(new { message = "Produto ou Fornecedor informado não existe no sistema." });
                if (affected == 0) return NotFound(new { message = "Compra não encontrada." });
                
                return Ok(new { message = "Compra atualizada com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao atualizar compra", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var affected = await ComprasRepository.DeleteComprasAsync(id);
                if (affected == 0) return NotFound(new { message = "Compra não encontrada." });
                
                return Ok(new { message = "Compra excluída com sucesso." });
            }
            catch (Exception ex)
            {
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 547 || ex is Microsoft.Data.SqlClient.SqlException seq && seq.Number == 547)
                {
                    return BadRequest(new { message = "Não é possível excluir esta compra pois ela pode estar relacionada com outras tabelas." });
                }
                return StatusCode(500, new { message = "Erro ao excluir compra", error = ex.Message });
            }
        }
    }
}
