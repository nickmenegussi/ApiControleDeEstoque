using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ApiControleEstoque.Models;
using ApiControleEstoque.Repository;
using ApiControleEstoque.Models.ViewModels;

namespace ApiControleEstoque.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimentacoesEstoqueController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var list = await MovimentacoesEstoqueRepository.GetAllAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar as movimentações.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var item = await MovimentacoesEstoqueRepository.GetByIdMovimentacaoAsync(id);
                if (item == null) return NotFound(new { message = "Movimentação não encontrada." });

                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar a movimentação.", error = ex.Message });
            }
        }

        [HttpGet("idEstoque/{idEstoque}")]
        public async Task<IActionResult> GetByIdEstoque(long idEstoque)
        {
            try
            {
                if (idEstoque <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var list = await MovimentacoesEstoqueRepository.SearchByFilterAsync(new FiltroParaMovimentacaoEstoque { IdEstoque = idEstoque });
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar movimentações por estoque.", error = ex.Message });
            }
        }

        [HttpGet("idProduto/{idProduto}")]
        public async Task<IActionResult> GetByIdProduto(long idProduto)
        {
            try
            {
                if (idProduto <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

                var list = await MovimentacoesEstoqueRepository.SearchByFilterAsync(new FiltroParaMovimentacaoEstoque { IdProduto = idProduto });
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar movimentações por produto.", error = ex.Message });
            }
        }

        [HttpGet("data/{dataInicio}/{dataFim}")]
        public async Task<IActionResult> GetByData(DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                var list = await MovimentacoesEstoqueRepository.SearchByFilterAsync(new FiltroParaMovimentacaoEstoque { DataInicio = dataInicio, DataFim = dataFim });
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar movimentações por período.", error = ex.Message });
            }
        }

        [HttpPost("tudo")]
        public async Task<IActionResult> PostTudo([FromBody] FiltroParaMovimentacaoEstoque filtro)
        {
            try
            {
                var list = await MovimentacoesEstoqueRepository.SearchByFilterAsync(filtro);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao filtrar movimentações.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] MovimentacoesEstoque movimentacao)
        {
            try
            {
                var result = await MovimentacoesEstoqueRepository.CreateMovimentacaoAsync(movimentacao);
                if (result == -1) return BadRequest(new { message = "Quantidade, e IDs de Produto/Estoque devem ser válidos e maiores que zero." });
                if (result == -2) return BadRequest(new { message = "Relacionamento inválido (Produto, Estoque ou Tipo de Movimentação não encontrados)." });

                return Created("", new { message = "Movimentação cadastrada com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao cadastrar movimentação.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] MovimentacoesEstoque movimentacao)
        {
            if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

            try
            {
                movimentacao.IdMovimentacaoEstoque = id;
                var result = await MovimentacoesEstoqueRepository.UpdateMovimentacaoAsync(movimentacao);
                
                if (result == 0) return NotFound(new { message = "Movimentação não encontrada." });
                if (result == -1) return BadRequest(new { message = "Dados inválidos." });
                if (result == -2) return BadRequest(new { message = "Relacionamento inválido (Produto, Estoque ou Tipo não encontrados)." });

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
            if (id <= 0) return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });

            try
            {
                var result = await MovimentacoesEstoqueRepository.DeleteMovimentacaoAsync(id);
                if (result == 0) return NotFound(new { message = "Movimentação não encontrada." });

                return Ok(new { message = "Movimentação excluída com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao excluir movimentação.", error = ex.Message });
            }
        }

        [HttpPost("venderProduto")]
        public async Task<IActionResult> VenderProduto([FromBody] MovimentacaoProdutoRequest request)
        {
            try
            {
                var result = await MovimentacoesEstoqueRepository.VenderOuDescartarAsync(request, 2); // Tipo 2 = Saída
                if (result == -1) return BadRequest(new { message = "Quantidade e IDs devem ser maiores que zero." });
                if (result == -2) return BadRequest(new { message = "Saldo insuficiente no estoque para esta venda." });
                if (result == -3) return BadRequest(new { message = "Produto ou Estoque inexistentes." });

                return Created("", new { message = "Venda realizada com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao processar venda.", error = ex.Message });
            }
        }

        [HttpPost("descartarProduto")]
        public async Task<IActionResult> DescartarProduto([FromBody] MovimentacaoProdutoRequest request)
        {
            try
            {
                var result = await MovimentacoesEstoqueRepository.VenderOuDescartarAsync(request, 3); // Tipo 3 = Descarte
                if (result == -1) return BadRequest(new { message = "Quantidade e IDs devem ser maiores que zero." });
                if (result == -2) return BadRequest(new { message = "Saldo insuficiente no estoque para este descarte." });
                if (result == -3) return BadRequest(new { message = "Produto ou Estoque inexistentes." });

                return Created("", new { message = "Descarte realizado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao processar descarte.", error = ex.Message });
            }
        }

        [HttpPost("transferirProduto")]
        public async Task<IActionResult> TransferirProduto([FromBody] TransferenciaEstoqueRequest request)
        {
            try
            {
                var result = await MovimentacoesEstoqueRepository.TransferirAsync(request);
                
                if (result == -1) return BadRequest(new { message = "Quantidade e IDs devem ser maiores que zero." });
                if (result == -3) return BadRequest(new { message = "Estoque de origem e destino não podem ser iguais." });
                if (result == -2) return BadRequest(new { message = "Saldo insuficiente no estoque de origem." });
                if (result == -4) return BadRequest(new { message = "Produto ou Estoque inexistentes." });

                return Ok(new { message = "Transferência realizada com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao processar transferência.", error = ex.Message });
            }
        }
    }
}
