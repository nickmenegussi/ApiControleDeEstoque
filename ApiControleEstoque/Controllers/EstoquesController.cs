using ApiControleEstoque.Models;
using ApiControleEstoque.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ApiControleEstoque.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstoquesController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var list = await EstoqueRepository.GetAllEstoqueAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar estoques", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var item = await EstoqueRepository.GetByIdAsync(id);
                if (item == null) return NotFound(new { message = "Estoque não encontrado." });
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar estoque", error = ex.Message });
            }
        }

        [HttpGet("tipo-estoque/{idTipoEstoque}")]
        public async Task<IActionResult> GetByIdTipoEstoque(long idTipoEstoque)
        {
            try
            {
                if (idTipoEstoque <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var list = await EstoqueRepository.SearchByFilterAsync(idTipoEstoque: idTipoEstoque);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar estoques por tipo", error = ex.Message });
            }
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> GetByDescricao([FromQuery] string descricao)
        {
            try
            {
                var list = await EstoqueRepository.SearchByFilterAsync(descricao: descricao);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar estoques por descrição", error = ex.Message });
            }
        }

        [HttpGet("{id}/quantidade-produtos")]
        public async Task<IActionResult> GetQuantidadeProdutosMapping(long id)
        {
            try
            {
                if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var result = await EstoqueRepository.GetQuantidadeProdutosNoEstoqueAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar quantidade de produtos no estoque", error = ex.Message });
            }
        }

        [HttpGet("{id}/recentes")]
        public async Task<IActionResult> GetRecentes(long id)
        {
            try
            {
                if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var result = await EstoqueRepository.GetMovimentacoesRecentesAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar movimentações recentes do estoque", error = ex.Message });
            }
        }

        [HttpGet("produto/{codBarra}")]
        public async Task<IActionResult> GetProdutoEmTodosEstoques(string codBarra)
        {
            try
            {
                var result = await EstoqueRepository.GetQuantidadeProdutoEmTodosEstoquesAsync(codBarra);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar produto em todos os estoques", error = ex.Message });
            }
        }

        [HttpPost("consultar")]
        public async Task<IActionResult> Consultar([FromBody] PesquisaEstoque vm)
        {
            try
            {
                var result = await EstoqueRepository.SearchByFilterAsync(idTipoEstoque: vm.IdTipoEstoque, descricao: vm.Descricao);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao filtrar estoques", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Estoques estoque)
        {
            try
            {
                var result = await EstoqueRepository.AddEstoqueAsync(estoque);
                if (result == 0) return BadRequest(new { message = "Já existe um estoque com esta descrição para este tipo." });
                if (result == -1) return BadRequest(new { message = "Descrição é obrigatória." });
                return Created("", new { message = "Estoque cadastrado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao cadastrar estoque", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] Estoques estoque)
        {
            try
            {
                if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                estoque.IdEstoque = id;
                var result = await EstoqueRepository.UpdateEstoqueAsync(estoque);
                if (result == 0) return BadRequest(new { message = "Já existe um estoque com esta descrição para este tipo." });
                if (result == -1) return BadRequest(new { message = "Descrição é obrigatória." });
                return Ok(new { message = "Estoque atualizado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao atualizar estoque", error = ex.Message });
            }
        }
    }
}
