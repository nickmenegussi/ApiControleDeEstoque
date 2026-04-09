using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiControleEstoque.Exceptions;
using ApiControleEstoque.Models;
using ApiControleEstoque.Repository;
using ApiControleEstoque.Models.ViewModels;

namespace ApiControleEstoque.Services
{
    /// <summary>
    /// Implementação do serviço de movimentações de estoque.
    /// Concentra todas as regras de negócio, deixando o Controller limpo.
    /// </summary>
    public class MovimentacoesEstoqueService : IMovimentacoesEstoqueService
    {
        private readonly IMovimentacoesEstoqueRepository _repository;

        public MovimentacoesEstoqueService(IMovimentacoesEstoqueRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        // ── Consultas ──────────────────────────────────────────────────────────

        public Task<IEnumerable<MovimentacoesEstoque>> ListarTodosAsync()
            => _repository.ListarTodosAsync();

        public Task<IEnumerable<MovimentacaoEstoqueView>> ListarTodosViewAsync()
            => _repository.ListarTodosViewAsync();

        public Task<MovimentacoesEstoque?> ConsultarPorIdAsync(long id)
            => _repository.ConsultarPorIdAsync(id);

        public Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorEstoqueAsync(long idEstoque)
            => _repository.ConsultarPorEstoqueAsync(idEstoque);

        public Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorEstoqueCompletoAsync(long idEstoque)
            => _repository.ConsultarPorEstoqueCompletoAsync(idEstoque);

        public Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorProdutoAsync(long idProduto)
            => _repository.ConsultarPorProdutoAsync(idProduto);

        public Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorProdutoCompletoAsync(long idProduto)
            => _repository.ConsultarPorProdutoCompletoAsync(idProduto);

        public Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorTipoMovimentacaoEstoqueAsync(long idTipo)
            => _repository.ConsultarPorTipoMovimentacaoEstoqueAsync(idTipo);

        public Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorFuncionarioSolicitadorAsync(long idFuncionario)
            => _repository.ConsultarPorFuncionarioSolicitadorAsync(idFuncionario);

        public Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorFuncionarioAutenticadorAsync(long idFuncionario)
            => _repository.ConsultarPorFuncionarioAutenticadorAsync(idFuncionario);

        public Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorDataAsync(DateTime dataInicio, DateTime dataFim)
            => _repository.ConsultarPorDataAsync(dataInicio, dataFim);

        public Task<IEnumerable<MovimentacaoEstoqueView>> ListarTodosViewComFiltroAsync(FiltroParaMovimentacaoEstoque filtro)
            => _repository.ListarTodosViewComFiltroAsync(filtro);

        // ── Comandos simples ───────────────────────────────────────────────────

        public Task<MovimentacoesEstoque> CadastrarAsync(MovimentacoesEstoque entidade)
            => _repository.CadastrarAsync(entidade);

        public async Task<MovimentacoesEstoque> AlterarAsync(MovimentacoesEstoque entidade)
        {
            // Regra: não pode alterar uma movimentação sem ID válido
            if (entidade.IdMovimentacaoEstoque <= 0)
                throw new ArgumentException("IdMovimentacaoEstoque deve ser maior que zero.");

            var existente = await _repository.ConsultarPorIdAsync(entidade.IdMovimentacaoEstoque);
            if (existente is null)
                throw new EntidadeNaoEncontradaException($"Movimentação {entidade.IdMovimentacaoEstoque} não encontrada.");

            return await _repository.AlterarAsync(entidade);
        }

        public async Task ExcluirPorIdAsync(long id)
        {
            var existente = await _repository.ConsultarPorIdAsync(id);
            if (existente is null)
                throw new EntidadeNaoEncontradaException($"Movimentação {id} não encontrada.");

            await _repository.ExcluirPorIdAsync(id);
        }

        // ── Operações de negócio complexas ─────────────────────────────────────

        /// <inheritdoc/>
        public async Task<MovimentacoesEstoque> VenderAsync(VMVendaEDescarte vm)
        {
            ValidarVMVendaEDescarte(vm);

            // Regra de negócio: quantidade em estoque deve ser suficiente
            var quantidadeDisponivel = await _repository.ObterQuantidadeDisponivelAsync(
                vm.IdProduto.Value, vm.IdEstoque.Value);

            if (vm.Quantidade > quantidadeDisponivel)
                throw new EstoqueInsuficienteException(
                    $"Estoque insuficiente. Disponível: {quantidadeDisponivel}, Solicitado: {vm.Quantidade}.");

            var movimentacao = MapearVendaParaMovimentacao(vm, TipoMovimentacao.Saida);
            return await _repository.CadastrarAsync(movimentacao);
        }

        /// <inheritdoc/>
        public async Task<MovimentacoesEstoque> DescartarAsync(VMVendaEDescarte vm)
        {
            ValidarVMVendaEDescarte(vm);

            // Regra de negócio: não se pode descartar mais do que existe
            var quantidadeDisponivel = await _repository.ObterQuantidadeDisponivelAsync(
                vm.IdProduto.Value, vm.IdEstoque.Value);

            if (vm.Quantidade > quantidadeDisponivel)
                throw new EstoqueInsuficienteException(
                    $"Quantidade para descarte ({vm.Quantidade}) excede o disponível ({quantidadeDisponivel}).");

            var movimentacao = MapearVendaParaMovimentacao(vm, TipoMovimentacao.Descarte);
            return await _repository.CadastrarAsync(movimentacao);
        }

        /// <inheritdoc/>
        public async Task FazerMovimentacaoAsync(VMMovimentacoesEstoque vm)
        {
            ValidarVMMovimentacao(vm);

            // Regra de negócio: estoque de origem deve ter quantidade suficiente
            var quantidadeDisponivel = await _repository.ObterQuantidadeDisponivelAsync(
                vm.IdProduto.Value, vm.IdEstoqueOrigem.Value);

            if (vm.Quantidade > quantidadeDisponivel)
                throw new EstoqueInsuficienteException(
                    $"Estoque de origem insuficiente. Disponível: {quantidadeDisponivel}, Solicitado: {vm.Quantidade}.");

            // Operação atômica: saída do estoque de origem + entrada no destino
            var saida = new MovimentacoesEstoque
            {
                IdEstoque              = vm.IdEstoqueOrigem,
                IdProduto              = vm.IdProduto,
                Quantidade             = (int)vm.Quantidade.Value,
                IdTipoMovimentacaoEstoque = (int)TipoMovimentacao.Saida,
                IdFuncionarioSolicitador = vm.IdFuncionarioSolicitador,
                Observacao             = $"Transferência para estoque {vm.IdEstoqueDestino}. {vm.Observacao}",
                DataHora               = DateTime.UtcNow
            };

            var entrada = new MovimentacoesEstoque
            {
                IdEstoque              = vm.IdEstoqueDestino,
                IdProduto              = vm.IdProduto,
                Quantidade             = (int)vm.Quantidade.Value,
                IdTipoMovimentacaoEstoque = (int)TipoMovimentacao.Entrada,
                IdFuncionarioSolicitador = vm.IdFuncionarioSolicitador,
                Observacao             = $"Transferência do estoque {vm.IdEstoqueOrigem}. {vm.Observacao}",
                DataHora               = DateTime.UtcNow
            };

            // Executadas dentro de uma única transação no repositório
            await _repository.ExecutarTransacaoAsync(new[] { saida, entrada });
        }

        // ── Helpers privados ───────────────────────────────────────────────────

        private static void ValidarVMVendaEDescarte(VMVendaEDescarte vm)
        {
            if (vm is null)                        throw new ArgumentNullException(nameof(vm));
            if (vm.IdProduto == null || vm.IdProduto <= 0)  throw new ArgumentException("IdProduto inválido.");
            if (vm.IdEstoque == null || vm.IdEstoque <= 0)  throw new ArgumentException("IdEstoque inválido.");
            if (vm.Quantidade == null || vm.Quantidade <= 0) throw new ArgumentException("Quantidade deve ser maior que zero.");
        }

        private static void ValidarVMMovimentacao(VMMovimentacoesEstoque vm)
        {
            if (vm is null)                             throw new ArgumentNullException(nameof(vm));
            if (vm.IdProduto == null || vm.IdProduto <= 0)       throw new ArgumentException("IdProduto inválido.");
            if (vm.IdEstoqueOrigem == null || vm.IdEstoqueOrigem <= 0)  throw new ArgumentException("IdEstoqueOrigem inválido.");
            if (vm.IdEstoqueDestino == null || vm.IdEstoqueDestino <= 0) throw new ArgumentException("IdEstoqueDestino inválido.");
            if (vm.IdEstoqueOrigem == vm.IdEstoqueDestino)
                throw new ArgumentException("Estoque de origem e destino não podem ser iguais.");
            if (vm.Quantidade == null || vm.Quantidade <= 0)     throw new ArgumentException("Quantidade deve ser maior que zero.");
        }

        private static MovimentacoesEstoque MapearVendaParaMovimentacao(VMVendaEDescarte vm, TipoMovimentacao tipo)
            => new MovimentacoesEstoque
            {
                IdEstoque                = vm.IdEstoque,
                IdProduto                = vm.IdProduto,
                Quantidade               = (int)vm.Quantidade.Value,
                IdTipoMovimentacaoEstoque = (int)tipo,
                IdFuncionarioSolicitador = vm.IdFuncionarioSolicitador,
                Observacao               = vm.Observacao,
                DataHora                 = DateTime.UtcNow
            };
    }
}
