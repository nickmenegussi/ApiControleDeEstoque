using System;
using System.Threading.Tasks;
using ApiControleEstoque.Models;
using ApiControleEstoque.Models.ViewModels;
using ApiControleEstoque.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ApiControleEstoque.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimentacoesEstoqueController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] MovimentacoesEstoque movimentacao)
        {
            try
            {
                var id = await MovimentacoesEstoqueRepository.CreateAsync(movimentacao);
                return Created("", new { message = "Movimentação registrada com sucesso.", id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao registrar movimentação.", error = ex.Message });
            }
        }

        [HttpPost("venderProduto")]
        public async Task<IActionResult> Vender([FromBody] VMVendaEDescarte vm)
        {
            try
            {

                var disponivel = await MovimentacoesEstoqueRepository.GetQuantidadeDisponivelAsync(vm.IdProduto, vm.IdEstoque);
                if (vm.Quantidade > disponivel)
                {
                    return BadRequest(new { message = $"Estoque insuficiente. Disponível: {disponivel}, Solicitado: {vm.Quantidade}" });
                }

                // ID 2 geralmente é Saída/Venda no sistema de moda
                await MovimentacoesEstoqueRepository.ExecutarVendaOuDescarteAsync(vm, 2);
                return Created("", new { message = "Venda registrada com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao registrar venda.", error = ex.Message });
            }
        }

        [HttpPost("descartarProduto")]
        public async Task<IActionResult> Descartar([FromBody] VMVendaEDescarte vm)
        {
            try
            {


                var disponivel = await MovimentacoesEstoqueRepository.GetQuantidadeDisponivelAsync(vm.IdProduto, vm.IdEstoque);
                if (vm.Quantidade > disponivel)
                {
                    return BadRequest(new { message = $"Quantidade insuficiente para descarte. Disponível: {disponivel}" });
                }

                // ID 3 geralmente é Descarte conforme lógica de saída
                await MovimentacoesEstoqueRepository.ExecutarVendaOuDescarteAsync(vm, 3);
                return Created("", new { message = "Descarte registrado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao registrar descarte.", error = ex.Message });
            }
        }

        [HttpPost("movimentarProdutoEntreEstoques")]
        public async Task<IActionResult> FazerMovimentacao([FromBody] VMMovimentacoesEstoque vm)
        {
            try
            {

                if (vm.IdEstoqueOrigem == vm.IdEstoqueDestino) return BadRequest(new { message = "O estoque de origem e destino não podem ser iguais." });

                var disponivel = await MovimentacoesEstoqueRepository.GetQuantidadeDisponivelAsync(vm.IdProduto, vm.IdEstoqueOrigem);
                if (vm.Quantidade > disponivel)
                {
                    return BadRequest(new { message = $"Estoque de origem insuficiente. Disponível: {disponivel}" });
                }

                await MovimentacoesEstoqueRepository.ExecutarTransferenciaAsync(vm);
                return Ok(new { message = "Transferência entre estoques realizada com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao realizar transferência.", error = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] MovimentacoesEstoque movimentacao)
        {
            try
            {
                if (movimentacao.IdMovimentacaoEstoque <= 0) return BadRequest(new { message = "ID inválido para atualização." });


                var affected = await MovimentacoesEstoqueRepository.UpdateAsync(movimentacao);
                if (affected == 0) return NotFound(new { message = "Movimentação não encontrada." });

                return Ok(new { message = "Movimentação atualizada com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao atualizar movimentação.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                if (id <= 0) return BadRequest(new { message = "ID deve ser maior que zero." });

                var affected = await MovimentacoesEstoqueRepository.DeleteAsync(id);
                if (affected == 0) return NotFound(new { message = "Movimentação não encontrada." });

                return Ok(new { message = "Movimentação excluída com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao excluir movimentação.", error = ex.Message });
            }
        }
    }
}
