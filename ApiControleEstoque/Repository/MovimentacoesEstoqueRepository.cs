using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ApiControleEstoque.Exceptions;
using ApiControleEstoque.Models;
using ApiControleEstoque.Models.ViewModels;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ApiControleEstoque.Repository
{
    // 1. A classe agora é estática
    public static class MovimentacoesEstoqueRepository
    {
        private static readonly string _connectionString;

        // 2. Construtor Estático (não tem modificador de acesso 'public')
        static MovimentacoesEstoqueRepository()
        {
            // Carregando as configurações diretamente do arquivo conforme o padrão do master
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = builder.Build();
            _connectionString = config.GetConnectionString("DefaultConnection") 
                                ?? throw new Exception("String de conexão 'DefaultConnection' não encontrada no appsettings.json");
        }

        // ── Queries de View ───────────────────────────────────────────────────

        private const string SQL_BASE_VIEW = @"
            SELECT 
                m.IdMovimentacaoEstoque,
                m.Quantidade,
                m.DataHora AS DataMovimentacao,
                t.Descricao AS TipoMovimentacao,
                p.Descricao AS ProdutoNome,
                e.Descricao AS EstoqueNome,
                fs.Nome AS FuncionarioSolicitador,
                fa.Nome AS FuncionarioAutenticador,
                m.Observacao
            FROM MovimentacoesEstoque m
            INNER JOIN TiposMovimentacaoEstoque t ON m.IdTipoMovimentacaoEstoque = t.IdTipoMovimentacaoEstoque
            INNER JOIN Produtos p ON m.IdProduto = p.IdProduto
            INNER JOIN Estoques e ON m.IdEstoque = e.IdEstoque
            INNER JOIN Funcionarios fs ON m.IdFuncionarioSolicitador = fs.IdFuncionario
            LEFT JOIN Funcionarios fa ON m.IdFuncionarioAutenticador = fa.IdFuncionario";

        // ── Métodos de Consulta (Static Async) ───────────────────────────────

        public static async Task<IEnumerable<MovimentacoesEstoque>> ListarTodosAsync()
        {
            string sql = "SELECT * FROM MovimentacoesEstoque";
            using var conexao = new SqlConnection(_connectionString);
            return await conexao.QueryAsync<MovimentacoesEstoque>(sql);
        }

        public static async Task<IEnumerable<MovimentacaoEstoqueView>> ListarTodosViewAsync()
        {
            using var conexao = new SqlConnection(_connectionString);
            return await conexao.QueryAsync<MovimentacaoEstoqueView>(SQL_BASE_VIEW);
        }

        public static async Task<MovimentacoesEstoque?> ConsultarPorIdAsync(long id)
        {
            string sql = "SELECT * FROM MovimentacoesEstoque WHERE IdMovimentacaoEstoque = @Id";
            using var conexao = new SqlConnection(_connectionString);
            return await conexao.QueryFirstOrDefaultAsync<MovimentacoesEstoque>(sql, new { Id = id });
        }

        public static async Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorEstoqueAsync(long idEstoque)
        {
            string sql = $"{SQL_BASE_VIEW} WHERE m.IdEstoque = @IdEstoque";
            using var conexao = new SqlConnection(_connectionString);
            return await conexao.QueryAsync<MovimentacaoEstoqueView>(sql, new { IdEstoque = idEstoque });
        }

        public static async Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorProdutoAsync(long idProduto)
        {
            string sql = $"{SQL_BASE_VIEW} WHERE m.IdProduto = @IdProduto";
            using var conexao = new SqlConnection(_connectionString);
            return await conexao.QueryAsync<MovimentacaoEstoqueView>(sql, new { IdProduto = idProduto });
        }

        public static async Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorDataAsync(DateTime dataInicio, DateTime dataFim)
        {
            string sql = $"{SQL_BASE_VIEW} WHERE m.DataHora BETWEEN @Inicio AND @Fim";
            using var conexao = new SqlConnection(_connectionString);
            return await conexao.QueryAsync<MovimentacaoEstoqueView>(sql, new { Inicio = dataInicio, Fim = dataFim });
        }

        public static async Task<IEnumerable<MovimentacaoEstoqueView>> ListarTodosViewComFiltroAsync(FiltroParaMovimentacaoEstoque filtro)
        {
            var sb = new StringBuilder(SQL_BASE_VIEW);
            sb.Append(" WHERE 1=1");
            var paramsObj = new DynamicParameters();

            if (filtro.IdEstoque.HasValue) { sb.Append(" AND m.IdEstoque = @IdEstoque"); paramsObj.Add("IdEstoque", filtro.IdEstoque); }
            if (filtro.IdProduto.HasValue) { sb.Append(" AND m.IdProduto = @IdProduto"); paramsObj.Add("IdProduto", filtro.IdProduto); }
            if (filtro.IdTipoMovimentacao.HasValue) { sb.Append(" AND m.IdTipoMovimentacaoEstoque = @IdTipo"); paramsObj.Add("IdTipo", filtro.IdTipoMovimentacao); }
            if (filtro.DataInicio.HasValue) { sb.Append(" AND m.DataHora >= @DataInicio"); paramsObj.Add("DataInicio", filtro.DataInicio); }
            if (filtro.DataFim.HasValue) { sb.Append(" AND m.DataHora <= @DataFim"); paramsObj.Add("DataFim", filtro.DataFim); }

            using var conexao = new SqlConnection(_connectionString);
            return await conexao.QueryAsync<MovimentacaoEstoqueView>(sb.ToString(), paramsObj);
        }

        // ── Comandos de Escrita (Static Async) ───────────────────────────────

        public static async Task<MovimentacoesEstoque> CadastrarAsync(MovimentacoesEstoque entidade)
        {
            const string sql = @"
                INSERT INTO MovimentacoesEstoque 
                (IdEstoque, IdTipoMovimentacaoEstoque, IdFuncionarioSolicitador, IdFuncionarioAutenticador, IdProduto, Quantidade, DataHora, Observacao)
                VALUES 
                (@IdEstoque, @IdTipoMovimentacaoEstoque, @IdFuncionarioSolicitador, @IdFuncionarioAutenticador, @IdProduto, @Quantidade, @DataHora, @Observacao);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";

            using var conexao = new SqlConnection(_connectionString);
            var id = await conexao.QuerySingleAsync<long>(sql, entidade);
            entidade.IdMovimentacaoEstoque = id;
            return entidade;
        }

        public static async Task<MovimentacoesEstoque> AlterarAsync(MovimentacoesEstoque entidade)
        {
            const string sql = @"
                UPDATE MovimentacoesEstoque SET 
                    IdEstoque = @IdEstoque, IdTipoMovimentacaoEstoque = @IdTipoMovimentacaoEstoque, 
                    IdFuncionarioSolicitador = @IdFuncionarioSolicitador, IdFuncionarioAutenticador = @IdFuncionarioAutenticador, 
                    IdProduto = @IdProduto, Quantidade = @Quantidade, DataHora = @DataHora, Observacao = @Observacao
                WHERE IdMovimentacaoEstoque = @IdMovimentacaoEstoque";

            using var conexao = new SqlConnection(_connectionString);
            await conexao.ExecuteAsync(sql, entidade);
            return entidade;
        }

        public static async Task ExcluirPorIdAsync(long id)
        {
            const string sql = "DELETE FROM MovimentacoesEstoque WHERE IdMovimentacaoEstoque = @Id";
            using var conexao = new SqlConnection(_connectionString);
            await conexao.ExecuteAsync(sql, new { Id = id });
        }

        // ── Lógica de Negócio ──────────────────────────────────────────────────

        public static async Task<long> ObterQuantidadeDisponivelAsync(long idProduto, long idEstoque)
        {
            const string sql = @"
                SELECT 
                    ISNULL(SUM(CASE WHEN IdTipoMovimentacaoEstoque IN (1, 4) THEN Quantidade ELSE 0 END), 0) - 
                    ISNULL(SUM(CASE WHEN IdTipoMovimentacaoEstoque IN (2, 3) THEN Quantidade ELSE 0 END), 0)
                FROM MovimentacoesEstoque
                WHERE IdProduto = @IdProduto AND IdEstoque = @IdEstoque";

            using var conexao = new SqlConnection(_connectionString);
            return (long)(await conexao.ExecuteScalarAsync<decimal?>(sql, new { IdProduto = idProduto, IdEstoque = idEstoque }) ?? 0);
        }

        public static async Task<MovimentacoesEstoque> VenderAsync(VMVendaEDescarte vm)
        {
            ValidarVMVendaEDescarte(vm);
            var qtdDisp = await ObterQuantidadeDisponivelAsync(vm.IdProduto.Value, vm.IdEstoque.Value);
            if (vm.Quantidade > qtdDisp) throw new EstoqueInsuficienteException("Estoque insuficiente.");

            var mov = MapearVendaParaMovimentacao(vm, TipoMovimentacao.Saida);
            return await CadastrarAsync(mov);
        }

        public static async Task<MovimentacoesEstoque> DescartarAsync(VMVendaEDescarte vm)
        {
            ValidarVMVendaEDescarte(vm);
            var qtdDisp = await ObterQuantidadeDisponivelAsync(vm.IdProduto.Value, vm.IdEstoque.Value);
            if (vm.Quantidade > qtdDisp) throw new EstoqueInsuficienteException("Quantidade insuficiente.");

            var mov = MapearVendaParaMovimentacao(vm, TipoMovimentacao.Descarte);
            return await CadastrarAsync(mov);
        }

        public static async Task TransferirAsync(VMMovimentacoesEstoque vm)
        {
            ValidarVMMovimentacao(vm);
            var qtdDisp = await ObterQuantidadeDisponivelAsync(vm.IdProduto.Value, vm.IdEstoqueOrigem.Value);
            if (vm.Quantidade > qtdDisp) throw new EstoqueInsuficienteException("Estoque de origem insuficiente.");

            var saida = new MovimentacoesEstoque {
                IdEstoque = vm.IdEstoqueOrigem, IdProduto = vm.IdProduto, Quantidade = (int)vm.Quantidade.Value,
                IdTipoMovimentacaoEstoque = (int)TipoMovimentacao.Saida, IdFuncionarioSolicitador = vm.IdFuncionarioSolicitador,
                Observacao = $"Transferência para {vm.IdEstoqueDestino}.", DataHora = DateTime.UtcNow
            };

            var entrada = new MovimentacoesEstoque {
                IdEstoque = vm.IdEstoqueDestino, IdProduto = vm.IdProduto, Quantidade = (int)vm.Quantidade.Value,
                IdTipoMovimentacaoEstoque = (int)TipoMovimentacao.Entrada, IdFuncionarioSolicitador = vm.IdFuncionarioSolicitador,
                Observacao = $"Transferência de {vm.IdEstoqueOrigem}.", DataHora = DateTime.UtcNow
            };

            await ExecutarTransacaoAsync(new[] { saida, entrada });
        }

        private static async Task ExecutarTransacaoAsync(IEnumerable<MovimentacoesEstoque> movimentacoes)
        {
            using var conexao = new SqlConnection(_connectionString);
            await conexao.OpenAsync();
            using var transacao = conexao.BeginTransaction();
            try {
                const string sql = @"
                    INSERT INTO MovimentacoesEstoque (IdEstoque, IdTipoMovimentacaoEstoque, IdFuncionarioSolicitador, IdFuncionarioAutenticador, IdProduto, Quantidade, DataHora, Observacao)
                    VALUES (@IdEstoque, @IdTipoMovimentacaoEstoque, @IdFuncionarioSolicitador, @IdFuncionarioAutenticador, @IdProduto, @Quantidade, @DataHora, @Observacao)";

                foreach (var m in movimentacoes) await conexao.ExecuteAsync(sql, m, transaction: transacao);
                transacao.Commit();
            } catch { transacao.Rollback(); throw; }
        }

        private static void ValidarVMVendaEDescarte(VMVendaEDescarte vm) {
            if (vm == null || vm.IdProduto <= 0 || vm.IdEstoque <= 0 || vm.Quantidade <= 0)
                throw new ArgumentException("Dados de movimentação inválidos.");
        }

        private static void ValidarVMMovimentacao(VMMovimentacoesEstoque vm) {
            if (vm == null || vm.IdProduto <= 0 || vm.IdEstoqueOrigem <= 0 || vm.IdEstoqueDestino <= 0 || vm.Quantidade <= 0)
                throw new ArgumentException("Dados de transferência inválidos.");
            if (vm.IdEstoqueOrigem == vm.IdEstoqueDestino) throw new ArgumentException("Origem e destino iguais.");
        }

        private static MovimentacoesEstoque MapearVendaParaMovimentacao(VMVendaEDescarte vm, TipoMovimentacao tipo)
            => new MovimentacoesEstoque {
                IdEstoque = vm.IdEstoque, IdProduto = vm.IdProduto, Quantidade = (int)vm.Quantidade.Value,
                IdTipoMovimentacaoEstoque = (int)tipo, IdFuncionarioSolicitador = vm.IdFuncionarioSolicitador,
                Observacao = vm.Observacao, DataHora = DateTime.UtcNow
            };
    }
}
