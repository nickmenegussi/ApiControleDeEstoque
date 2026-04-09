using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiControleEstoque.Models;
using ApiControleEstoque.Models.ViewModels;

namespace ApiControleEstoque.Services
{
    /// <summary>
    /// Contrato da camada de serviço para movimentações de estoque.
    /// Responsabilidade: orquestrar regras de negócio, validações e chamadas ao repositório.
    /// O Controller apenas delega; toda lógica de negócio vive aqui ou no domínio.
    /// </summary>
    public interface IMovimentacoesEstoqueService
    {
        // ── Consultas ──────────────────────────────────────────────────────────

        Task<IEnumerable<MovimentacoesEstoque>> ListarTodosAsync();
        Task<IEnumerable<MovimentacaoEstoqueView>> ListarTodosViewAsync();
        Task<MovimentacoesEstoque?> ConsultarPorIdAsync(long id);
        Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorEstoqueAsync(long idEstoque);
        Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorEstoqueCompletoAsync(long idEstoque);
        Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorProdutoAsync(long idProduto);
        Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorProdutoCompletoAsync(long idProduto);
        Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorTipoMovimentacaoEstoqueAsync(long idTipo);
        Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorFuncionarioSolicitadorAsync(long idFuncionario);
        Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorFuncionarioAutenticadorAsync(long idFuncionario);
        Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorDataAsync(DateTime dataInicio, DateTime dataFim);
        Task<IEnumerable<MovimentacaoEstoqueView>> ListarTodosViewComFiltroAsync(FiltroParaMovimentacaoEstoque filtro);

        // ── Comandos simples ───────────────────────────────────────────────────

        Task<MovimentacoesEstoque> CadastrarAsync(MovimentacoesEstoque entidade);
        Task<MovimentacoesEstoque> AlterarAsync(MovimentacoesEstoque entidade);
        Task ExcluirPorIdAsync(long id);

        // ── Operações de negócio complexas ─────────────────────────────────────

        /// <summary>
        /// Registra uma venda de produto, validando estoque suficiente antes de confirmar.
        /// Lança <see cref="EstoqueInsuficienteException"/> se a quantidade for inadequada.
        /// </summary>
        Task<MovimentacoesEstoque> VenderAsync(VMVendaEDescarte vm);

        /// <summary>
        /// Registra o descarte (perda/vencimento) de um produto, validando quantidade disponível.
        /// </summary>
        Task<MovimentacoesEstoque> DescartarAsync(VMVendaEDescarte vm);

        /// <summary>
        /// Transfere produto entre dois estoques de forma atômica:
        /// remove do estoque de origem e adiciona no de destino na mesma transação.
        /// </summary>
        Task FazerMovimentacaoAsync(VMMovimentacoesEstoque vm);
    }
}
