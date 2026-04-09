using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiControleEstoque.Models;
using ApiControleEstoque.Models.ViewModels;

namespace ApiControleEstoque.Repository
{
    /// <summary>
    /// Contrato de acesso a dados para MovimentacoesEstoque.
    /// Responsabilidade única: operações de persistência (CRUD + queries).
    /// </summary>
    public interface IMovimentacoesEstoqueRepository
    {
        // ── Queries ────────────────────────────────────────────────────────────

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

        // ── Consultas de suporte (para validações de negócio no Service) ───────

        /// <summary>Retorna a quantidade atual disponível de um produto em um estoque específico.</summary>
        Task<decimal> ObterQuantidadeDisponivelAsync(long idProduto, long idEstoque);

        // ── Comandos ───────────────────────────────────────────────────────────

        Task<MovimentacoesEstoque> CadastrarAsync(MovimentacoesEstoque entidade);
        Task<MovimentacoesEstoque> AlterarAsync(MovimentacoesEstoque entidade);
        Task ExcluirPorIdAsync(long id);

        /// <summary>
        /// Executa múltiplas movimentações em uma única transação de banco de dados.
        /// Usado para garantir atomicidade em transferências entre estoques.
        /// </summary>
        Task ExecutarTransacaoAsync(IEnumerable<MovimentacoesEstoque> movimentacoes);
    }
}
