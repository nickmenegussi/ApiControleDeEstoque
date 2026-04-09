using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ApiControleEstoque.Exceptions;
using ApiControleEstoque.Models;
using ApiControleEstoque.Repository;
using ApiControleEstoque.Models.ViewModels;

namespace ApiControleEstoque.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimentacoesEstoqueController : ControllerBase
    {
        // ATENÇÃO: O construtor com injeção de dependência foi removido!

        [HttpGet]
        public async Task<IActionResult> ListarTodos()
        {
            try
            {
                var dados = await MovimentacoesEstoqueRepository.ListarTodosAsync();
                return Ok(new { message = "Movimentações listadas com sucesso.", data = dados });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao buscar as movimentações.", error = ex.Message });
            }
        }

        [HttpGet("view")]
        public async Task<IActionResult> ListarTodosView()
        {
            try
            {
                var dados = await MovimentacoesEstoqueRepository.ListarTodosViewAsync();
                return Ok(new { message = "View de movimentações listada com sucesso.", data = dados });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao buscar a view de movimentações.", error = ex.Message });
            }
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> ConsultarPorId(long id)
        {
            try
            {
                var movimentacao = await MovimentacoesEstoqueRepository.ConsultarPorIdAsync(id);

                if (movimentacao == null)
                {
                    return NotFound(new { message = $"Movimentação {id} não encontrada." });
                }

                return Ok(new { message = "Movimentação encontrada.", data = movimentacao });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao buscar a movimentação.", error = ex.Message });
            }
        }

        [HttpGet("estoque/{idEstoque}")]
        public async Task<IActionResult> ConsultarPorEstoque(long idEstoque)
        {
            try
            {
                var dados = await MovimentacoesEstoqueRepository.ConsultarPorEstoqueAsync(idEstoque);
                return Ok(new { message = "Busca por estoque concluída.", data = dados });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar movimentações pelo estoque.", error = ex.Message });
            }
        }

        [HttpGet("produto/{idProduto}")]
        public async Task<IActionResult> ConsultarPorProduto(long idProduto)
        {
            try
            {
                var dados = await MovimentacoesEstoqueRepository.ConsultarPorProdutoAsync(idProduto);
                return Ok(new { message = "Busca por produto concluída.", data = dados });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar movimentações pelo produto.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] MovimentacoesEstoque entidade)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Dados inválidos para cadastro.", error = "ModelState inválido." });

                var criado = await MovimentacoesEstoqueRepository.CadastrarAsync(entidade);
                return Created("", new { message = "Movimentação cadastrada com sucesso.", data = criado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao cadastrar movimentação.", error = ex.Message });
            }
        }

        [HttpPost("filtrar")]
        public async Task<IActionResult> ListarTodosViewComFiltro([FromBody] FiltroParaMovimentacaoEstoque filtro)
        {
            try
            {
                var dados = await MovimentacoesEstoqueRepository.ListarTodosViewComFiltroAsync(filtro);
                return Ok(new { message = "Filtro aplicado com sucesso.", data = dados });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao filtrar movimentações.", error = ex.Message });
            }
        }

        [HttpPost("venderProduto")]
        public async Task<IActionResult> Vender([FromBody] VMVendaEDescarte vm)
        {
            try
            {
                var criado = await MovimentacoesEstoqueRepository.VenderAsync(vm);
                return Created("", new { message = "Venda realizada com sucesso.", data = criado });
            }
            catch (EstoqueInsuficienteException ex)
            {
                return BadRequest(new { message = "Estoque insuficiente para a venda.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao processar venda.", error = ex.Message });
            }
        }

        [HttpPost("descartarProduto")]
        public async Task<IActionResult> Descartar([FromBody] VMVendaEDescarte vm)
        {
            try
            {
                var criado = await MovimentacoesEstoqueRepository.DescartarAsync(vm);
                return Created("", new { message = "Descarte realizado com sucesso.", data = criado });
            }
            catch (EstoqueInsuficienteException ex)
            {
                return BadRequest(new { message = "Quantidade indisponível para descarte.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao processar descarte.", error = ex.Message });
            }
        }

        [HttpPost("movimentarProdutoEntreEstoques")]
        public async Task<IActionResult> FazerMovimentacao([FromBody] VMMovimentacoesEstoque vm)
        {
            try
            {
                await MovimentacoesEstoqueRepository.TransferirAsync(vm);
                return Ok(new { message = "Transferência entre estoques realizada com sucesso.", data = vm });
            }
            catch (EstoqueInsuficienteException ex)
            {
                return BadRequest(new { message = "Estoque de origem insuficiente para transferência.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao processar transferência.", error = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Alterar([FromBody] MovimentacoesEstoque entidade)
        {
            try
            {
                if (entidade.IdMovimentacaoEstoque <= 0)
                    return BadRequest(new { message = "ID da movimentação inválido para alteração." });

                var atualizado = await MovimentacoesEstoqueRepository.AlterarAsync(entidade);
                return Ok(new { message = "Movimentação alterada com sucesso.", data = atualizado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao alterar movimentação.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirPorId(long id)
        {
            try
            {
                await MovimentacoesEstoqueRepository.ExcluirPorIdAsync(id);
                return Ok(new { message = "Movimentação excluída com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao excluir movimentação.", error = ex.Message });
            }
        }
    }
}
