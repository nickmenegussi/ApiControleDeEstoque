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
                if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

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
        public async Task<IActionResult> Post([FromBody] Produtos produto)
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
        public async Task<IActionResult> Put(long id, [FromBody] Produtos produto)
        {
            try
            {
                if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

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

        [HttpGet("descricao/{descricao}")]
        public async Task<IActionResult> GetByDescricao(string descricao)
        {
            try
            {
                var list = await ProdutosRepository.ConsultarPorDescricaoAsync(descricao);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar produtos por descrição", error = ex.Message });
            }
        }

        [HttpGet("quantPorEstoque/{idProduto}")]
        public async Task<IActionResult> GetQuantPorEstoque(long idProduto)
        {
            try
            {
                if (idProduto <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var result = await ProdutosRepository.ListarQuantidadePorEstoqueAsync(idProduto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar quantidade por estoque", error = ex.Message });
            }
        }

        [HttpGet("movimentacoesRecentes/{idProduto}")]
        public async Task<IActionResult> GetMovimentacoesRecentes(long idProduto)
        {
            try
            {
                if (idProduto <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var result = await ProdutosRepository.ListarMovimentacoesRecentesAsync(idProduto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar movimentações recentes do produto", error = ex.Message });
            }
        }

        [HttpPost("tudo")]
        public async Task<IActionResult> PostTudo([FromBody] PesquisaPadrao pesquisa)
        {
            try
            {
                var list = await ProdutosRepository.ConsultarPorTudoAsync(pesquisa.Query);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro na pesquisa geral de produtos", error = ex.Message });
            }
        }

        [HttpPost("descricaoECodBarras")]
        public async Task<IActionResult> PostDescricaoECodBarras([FromBody] PesquisaDescricaoCodBarra pesquisa)
        {
            try
            {
                var list = await ProdutosRepository.ConsultarPorDescricaoECodBarrasAsync(pesquisa.Descricao, pesquisa.CodBarra);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro na pesquisa de produtos por descrição e código de barras", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

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
                    return BadRequest(new { message = "Este produto não pode ser excluído diretamente pois existem Compras ou Movimentações registradas com ele." });
                }
                return StatusCode(500, new { message = "Erro ao excluir produto", error = ex.Message });
            }
        }
    }
}
