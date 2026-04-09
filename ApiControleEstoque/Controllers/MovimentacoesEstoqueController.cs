using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ApiControleEstoque.Exceptions;
using ApiControleEstoque.Models;
using ApiControleEstoque.Services;
using ApiControleEstoque.Models.ViewModels;

namespace ApiControleEstoque.Controllers
{
    /// <summary>
    /// Controller responsável por expor os endpoints de movimentações de estoque.
    ///
    /// RESPONSABILIDADE: Apenas orquestrar HTTP — receber, delegar ao service,
    /// e retornar a resposta adequada. Nenhuma regra de negócio reside aqui.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MovimentacoesEstoqueController : ControllerBase
    {
        private readonly IMovimentacoesEstoqueService _service;
        private readonly ILogger<MovimentacoesEstoqueController> _logger;

        public MovimentacoesEstoqueController(
            IMovimentacoesEstoqueService service,
            ILogger<MovimentacoesEstoqueController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
        }

        // ══════════════════════════════════════════════════════════════════════
        //  GET — Consultas
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>Retorna todas as movimentações (dados brutos).</summary>
        /// <response code="200">Lista retornada com sucesso.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListarTodos()
        {
            try
            {
                var resultado = await _service.ListarTodosAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar todas as movimentações.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }

        /// <summary>Retorna todas as movimentações enriquecidas (dados de view).</summary>
        /// <response code="200">Lista de view retornada com sucesso.</response>
        [HttpGet("view")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListarTodosView()
        {
            try
            {
                var resultado = await _service.ListarTodosViewAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar todas as movimentações (view).");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }

        /// <summary>Busca uma movimentação pelo seu identificador.</summary>
        /// <param name="id">Identificador único da movimentação.</param>
        /// <response code="200">Movimentação encontrada.</response>
        /// <response code="404">Movimentação não encontrada.</response>
        [HttpGet("id/{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultarPorId(long id)
        {
            try
            {
                var resultado = await _service.ConsultarPorIdAsync(id);
                return resultado is null ? NotFound($"Movimentação {id} não encontrada.") : Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar movimentação por ID {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }

        /// <summary>Lista movimentações filtradas por estoque.</summary>
        /// <param name="idEstoque">Identificador do estoque.</param>
        [HttpGet("estoque/{idEstoque:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultarPorEstoque(long idEstoque)
        {
            try
            {
                var resultado = await _service.ConsultarPorEstoqueAsync(idEstoque);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar movimentações por estoque {IdEstoque}.", idEstoque);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }

        /// <summary>Lista movimentações completas (join com dados do estoque) filtradas por estoque.</summary>
        /// <param name="idEstoque">Identificador do estoque.</param>
        [HttpGet("estoqueCompleto/{idEstoque:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultarPorEstoqueCompleto(long idEstoque)
        {
            try
            {
                var resultado = await _service.ConsultarPorEstoqueCompletoAsync(idEstoque);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar movimentações completas por estoque {IdEstoque}.", idEstoque);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }

        /// <summary>Lista movimentações filtradas por produto.</summary>
        /// <param name="idProduto">Identificador do produto.</param>
        [HttpGet("produto/{idProduto:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultarPorProduto(long idProduto)
        {
            try
            {
                var resultado = await _service.ConsultarPorProdutoAsync(idProduto);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar movimentações por produto {IdProduto}.", idProduto);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }

        /// <summary>Lista movimentações completas (join com dados do produto) filtradas por produto.</summary>
        /// <param name="idProduto">Identificador do produto.</param>
        [HttpGet("produtoCompleto/{idProduto:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultarPorProdutoCompleto(long idProduto)
        {
            try
            {
                var resultado = await _service.ConsultarPorProdutoCompletoAsync(idProduto);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar movimentações completas por produto {IdProduto}.", idProduto);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }

        /// <summary>Lista movimentações por tipo (entrada, saída, descarte, etc.).</summary>
        /// <param name="idTipo">Identificador do tipo de movimentação.</param>
        [HttpGet("tipoMovimentacaoEstoque/{idTipo:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultarPorTipoMovimentacaoEstoque(long idTipo)
        {
            try
            {
                var resultado = await _service.ConsultarPorTipoMovimentacaoEstoqueAsync(idTipo);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar movimentações por tipo {IdTipo}.", idTipo);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }

        /// <summary>Lista movimentações solicitadas por um funcionário.</summary>
        /// <param name="id">Identificador do funcionário solicitador.</param>
        [HttpGet("funcionarioSolicitador/{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultarPorFuncionarioSolicitador(long id)
        {
            try
            {
                var resultado = await _service.ConsultarPorFuncionarioSolicitadorAsync(id);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar por funcionário solicitador {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }

        /// <summary>Lista movimentações autenticadas por um funcionário.</summary>
        /// <param name="id">Identificador do funcionário autenticador.</param>
        [HttpGet("funcionarioAutenticador/{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultarPorFuncionarioAutenticador(long id)
        {
            try
            {
                var resultado = await _service.ConsultarPorFuncionarioAutenticadorAsync(id);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar por funcionário autenticador {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }

        /// <summary>Lista movimentações dentro de um intervalo de datas.</summary>
        /// <param name="inicio">Data inicial (formato: yyyy-MM-dd).</param>
        /// <param name="fim">Data final (formato: yyyy-MM-dd).</param>
        [HttpGet("data/{inicio:datetime}/{fim:datetime}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultarPorData(DateTime inicio, DateTime fim)
        {
            try
            {
                if (inicio > fim)
                    return BadRequest("A data de início não pode ser posterior à data de fim.");

                var resultado = await _service.ConsultarPorDataAsync(inicio, fim);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar movimentações por data ({Inicio} - {Fim}).", inicio, fim);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  POST — Comandos
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>Cadastra uma nova movimentação simples.</summary>
        /// <param name="entidade">Dados da movimentação a ser cadastrada.</param>
        /// <response code="201">Movimentação criada com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Cadastrar([FromBody] MovimentacoesEstoque entidade)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var criado = await _service.CadastrarAsync(entidade);
                return CreatedAtAction(
                    nameof(ConsultarPorId),
                    new { id = criado.IdMovimentacaoEstoque },
                    criado);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Dados inválidos ao cadastrar movimentação.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cadastrar movimentação.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }

        /// <summary>
        /// Retorna movimentações (view enriquecida) aplicando filtros avançados.
        /// Permite combinar data, produto, estoque, tipo e quantidade.
        /// </summary>
        /// <param name="filtro">Objeto com os critérios de filtragem.</param>
        [HttpPost("filtrar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListarTodosViewComFiltro([FromBody] FiltroParaMovimentacaoEstoque filtro)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var resultado = await _service.ListarTodosViewComFiltroAsync(filtro);
                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Filtro inválido ao buscar movimentações.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao filtrar movimentações.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }

        /// <summary>
        /// Registra a venda de um produto.
        /// Valida se há quantidade suficiente em estoque antes de confirmar.
        /// </summary>
        /// <param name="vm">Dados da venda (produto, estoque, quantidade).</param>
        /// <response code="201">Venda registrada com sucesso.</response>
        /// <response code="400">Dados inválidos ou estoque insuficiente.</response>
        [HttpPost("venderProduto")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Vender([FromBody] VMVendaEDescarte vm)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var resultado = await _service.VenderAsync(vm);
                return CreatedAtAction(
                    nameof(ConsultarPorId),
                    new { id = resultado.IdMovimentacaoEstoque },
                    resultado);
            }
            catch (EstoqueInsuficienteException ex)
            {
                _logger.LogWarning(ex, "Tentativa de venda com estoque insuficiente.");
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Dados inválidos na venda de produto.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao registrar venda.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }

        /// <summary>
        /// Registra o descarte (perda ou vencimento) de um produto.
        /// Valida a quantidade disponível antes de confirmar.
        /// </summary>
        /// <param name="vm">Dados do descarte (produto, estoque, quantidade, motivo).</param>
        /// <response code="201">Descarte registrado com sucesso.</response>
        /// <response code="400">Dados inválidos ou estoque insuficiente.</response>
        [HttpPost("descartarProduto")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Descartar([FromBody] VMVendaEDescarte vm)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var resultado = await _service.DescartarAsync(vm);
                return CreatedAtAction(
                    nameof(ConsultarPorId),
                    new { id = resultado.IdMovimentacaoEstoque },
                    resultado);
            }
            catch (EstoqueInsuficienteException ex)
            {
                _logger.LogWarning(ex, "Tentativa de descarte com estoque insuficiente.");
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Dados inválidos no descarte de produto.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao registrar descarte.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }

        /// <summary>
        /// Transfere produto de um estoque para outro de forma atômica.
        /// Remove do estoque de origem e insere no destino na mesma transação.
        /// </summary>
        /// <param name="vm">Dados da transferência (produto, estoque origem, estoque destino, quantidade).</param>
        /// <response code="200">Transferência concluída com sucesso.</response>
        /// <response code="400">Dados inválidos ou estoque de origem insuficiente.</response>
        [HttpPost("movimentarProdutoEntreEstoques")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FazerMovimentacao([FromBody] VMMovimentacoesEstoque vm)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _service.FazerMovimentacaoAsync(vm);

                return Ok(new
                {
                    Mensagem    = "Transferência concluída com sucesso.",
                    IdProduto   = vm.IdProduto,
                    Origem      = vm.IdEstoqueOrigem,
                    Destino     = vm.IdEstoqueDestino,
                    Quantidade  = vm.Quantidade
                });
            }
            catch (EstoqueInsuficienteException ex)
            {
                _logger.LogWarning(ex, "Tentativa de transferência com estoque insuficiente.");
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Dados inválidos na transferência entre estoques.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao transferir produto entre estoques.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PUT — Atualização
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>Altera uma movimentação existente.</summary>
        /// <param name="entidade">Entidade com os dados atualizados. IdMovimentacaoEstoque deve ser > 0.</param>
        /// <response code="200">Movimentação alterada com sucesso.</response>
        /// <response code="400">ID inválido ou dados incorretos.</response>
        /// <response code="404">Movimentação não encontrada.</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Alterar([FromBody] MovimentacoesEstoque entidade)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Validação explícita — detalhada aqui para resposta clara ao cliente
                if (entidade.IdMovimentacaoEstoque <= 0)
                    return BadRequest("IdMovimentacaoEstoque deve ser maior que zero.");

                var atualizado = await _service.AlterarAsync(entidade);
                return Ok(atualizado);
            }
            catch (EntidadeNaoEncontradaException ex)
            {
                _logger.LogWarning(ex, "Movimentação não encontrada para alteração.");
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Dados inválidos na alteração de movimentação.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar movimentação.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  DELETE — Exclusão
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>Exclui uma movimentação pelo seu identificador.</summary>
        /// <param name="id">Identificador da movimentação a ser excluída.</param>
        /// <response code="204">Excluído com sucesso (sem conteúdo).</response>
        /// <response code="404">Movimentação não encontrada.</response>
        [HttpDelete("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExcluirPorId(long id)
        {
            try
            {
                await _service.ExcluirPorIdAsync(id);
                return NoContent();
            }
            catch (EntidadeNaoEncontradaException ex)
            {
                _logger.LogWarning(ex, "Movimentação {Id} não encontrada para exclusão.", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir movimentação {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar a requisição.");
            }
        }
    }
}
