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
                var list = await ComprasRepository.GetAllComprasAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar compras", error = ex.Message });
            }
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var item = await ComprasRepository.GetByIdComprasAsync(id);
                if (item == null) return NotFound(new { message = "Compra não encontrada." });
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar compra", error = ex.Message });
            }
        }

        [HttpGet("idFornecedor/{idFornecedor}")]
        public async Task<IActionResult> GetByIdFornecedor(long idFornecedor)
        {
            try
            {
                if (idFornecedor <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var list = await ComprasRepository.GetComprasByFornecedorAsync(idFornecedor);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar compras por fornecedor", error = ex.Message });
            }
        }

        [HttpGet("idProduto/{idProduto}")]
        public async Task<IActionResult> GetByIdProduto(long idProduto)
        {
            try
            {
                if (idProduto <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var list = await ComprasRepository.GetComprasByProdutoIdAsync(idProduto);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar compras por produto", error = ex.Message });
            }
        }

        [HttpGet("data/{dataInicio}/{dataFim}")]
        public async Task<IActionResult> GetByData(DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                var list = await ComprasRepository.GetComprasByPeriodoAsync(dataInicio, dataFim);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar compras por período", error = ex.Message });
            }
        }

        [HttpGet("ConsultarCompraCompleta/{id}")]
        public async Task<IActionResult> GetCompraCompleta(long id)
        {
            try
            {
                if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var item = await ComprasRepository.GetCompraCompletaAsync(id);
                if (item == null) return NotFound(new { message = "Compra não encontrada." });
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar compra completa", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Compras compra)
        {
            try
            {
                var result = await ComprasRepository.CreateCompraAsync(compra);
                if (result == -1) return BadRequest(new { message = "Quantidade deve ser maior que zero." });
                if (result == -2) return BadRequest(new { message = "Fornecedor ou Produto inexistentes." });
                return Created("", new { message = "Compra cadastrada com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao cadastrar compra simples", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] Compras compra)
        {
            if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

            try
            {
                compra.IdCompra = id;
                var result = await ComprasRepository.UpdateComprasAsync(compra);
                if (result == 0) return NotFound(new { message = "Compra não encontrada." });
                if (result == -1) return BadRequest(new { message = "Dados inválidos ou Quantidade zerada." });
                if (result == -2) return BadRequest(new { message = "Fornecedor ou Produto inexistentes." });
                return Ok(new { message = "Compra atualizada com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao atualizar compra", error = ex.Message });
            }
        }

        [HttpPost("tudo")]
        public async Task<IActionResult> PostTudo([FromBody] FiltroParaCompra filtro)
        {
            try
            {
                var list = await ComprasRepository.ConsultarPorTudoAsync(filtro);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao filtrar compras", error = ex.Message });
            }
        }

        [HttpPost("Compra")]
        public async Task<IActionResult> Comprar([FromBody] SolicitacaoCompra solicitacao)
        {
            try
            {
                var result = await ComprasRepository.ComprarAsync(solicitacao);
                if (result == -1) return BadRequest(new { message = "Quantidade deve ser maior que zero." });
                return Created("", new { message = "Compra realizada e estoque atualizado com sucesso.", idCompra = result });
            }
            catch (Exception ex)
            {
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 547 || ex is Microsoft.Data.SqlClient.SqlException seq && seq.Number == 547)
                {
                    return BadRequest(new { message = "Não foi possível realizar a compra. Verifique se o Produto, Fornecedor e Funcionário existem." });
                }
                return StatusCode(500, new { message = "Erro ao processar compra mestre", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

            try
            {
                var result = await ComprasRepository.DeleteComprasAsync(id);
                if (result == 0) return NotFound(new { message = "Compra não encontrada." });
                return Ok(new { message = "Compra excluída com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao excluir compra", error = ex.Message });
            }
        }
    }
}
