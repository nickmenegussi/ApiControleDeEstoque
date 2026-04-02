using ApiControleEstoque.Models;
using ApiControleEstoque.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ApiControleEstoque.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var produtos = await ProdutosRepository.GetAllProdutosAsync();
                return Ok(produtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar produtos", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                var produto = await ProdutosRepository.GetByIdAsync(id);
                if (produto == null) return NotFound(new { message = "Produto não encontrado." });
                return Ok(produto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar produto", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Produto produto)
        {
            try
            {
                var result = await ProdutosRepository.CreateProdutoAsync(produto);
                if (result == -1) return BadRequest(new { message = "Dados inválidos: Código de Barras e Descrição são obrigatórios." });
                if (result == 0) return BadRequest(new { message = "Não foi possível criar o produto. O código de barras já existe." });

                return Created("", new { message = "Produto criado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao criar produto", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] Produto produto)
        {
            try
            {
                produto.IdProduto = id;
                var result = await ProdutosRepository.UpdateProdutoAsync(produto);
                if (result == -1) return BadRequest(new { message = "Dados inválidos: Código de Barras e Descrição são obrigatórios." });
                if (result == 0) return BadRequest(new { message = "Não foi possível atualizar o produto. O código de barras já existe em outro produto." });

                return Ok(new { message = "Produto atualizado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao atualizar produto", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var result = await ProdutosRepository.DeleteProdutoAsync(id);
                if (result == 0) return NotFound(new { message = "Produto não encontrado." });

                return Ok(new { message = "Produto excluído com sucesso." });
            }
            catch (Exception ex)
            {
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 547 || ex is Microsoft.Data.SqlClient.SqlException seq && seq.Number == 547)
                {
                    return BadRequest(new { 
                        message = "Este produto não pode ser excluído diretamente pois existem Compras ou Movimentações registradas com ele.",
                        suggestion = "Para excluir este produto, você precisa excluir as compras associadas a ele primeiro. Deseja prosseguir com a exclusão das dependências?"
                    });
                }
                return StatusCode(500, new { message = "Erro ao excluir produto", error = ex.Message });
            }
        }
    }
}
