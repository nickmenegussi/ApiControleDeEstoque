using ApiControleEstoque.Models;
using ApiControleEstoque.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ApiControleEstoque.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FornecedoresController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var list = await FornecedoresRepository.GetAllAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar fornecedores", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var item = await FornecedoresRepository.GetByIdFornecedorAsync(id);
                if (item == null) return NotFound(new { message = "Fornecedor não encontrado." });
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar fornecedor", error = ex.Message });
            }
        }

        [HttpGet("cnpj/{cnpj}")]
        public async Task<IActionResult> GetByCnpj(string cnpj)
        {
            try
            {
                var item = await FornecedoresRepository.GetByCNPJAsync(cnpj);
                if (item == null) return NotFound(new { message = "Fornecedor não encontrado." });
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar fornecedor por CNPJ", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Fornecedores fornecedor)
        {
            try
            {
                var result = await FornecedoresRepository.AddAsync(fornecedor);
                if (result == 0) return BadRequest(new { message = "CNPJ já cadastrado." });
                if (result == -1) return BadRequest(new { message = "Dados inválidos (CNPJ ou Nome vazios)." });
                return Created("", new { message = "Fornecedor cadastrado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao cadastrar fornecedor", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] Fornecedores fornecedor)
        {
            try
            {
                if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                fornecedor.IdFornecedor = id;
                var result = await FornecedoresRepository.UpdateFornecedorAsync(fornecedor);
                if (result == 0) return BadRequest(new { message = "CNPJ já cadastrado para outro fornecedor." });
                if (result == -1) return BadRequest(new { message = "Dados inválidos (CNPJ ou Nome vazios)." });
                return Ok(new { message = "Fornecedor atualizado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao atualizar fornecedor", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

            try
            {
                var result = await FornecedoresRepository.DeleteFornecedoresByIdAsync(id);
                if (result == 0) return NotFound(new { message = "Fornecedor não encontrado." });
                return Ok(new { message = "Fornecedor excluído com sucesso." });
            }
            catch (Exception ex)
            {
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 547 || ex is Microsoft.Data.SqlClient.SqlException seq && seq.Number == 547)
                {
                    return BadRequest(new { message = "Não é possível excluir o fornecedor pois ele já possui vínculo em compras." });
                }
                return StatusCode(500, new { message = "Erro ao excluir fornecedor", error = ex.Message });
            }
        }
    }
}
