using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Dapper;
using ApiControleEstoque.Models;
using ApiControleEstoque.Models.ViewModels;

namespace ApiControleEstoque.Repository
{
    public class MovimentacoesEstoqueRepository : IMovimentacoesEstoqueRepository
    {
        private readonly string _connectionString;

        public MovimentacoesEstoqueRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                                ?? throw new ArgumentNullException("A string de conexão não foi encontrada.");
        }

        // ── Queries Simples ───────────────────────────────────────────────────

        public async Task<IEnumerable<MovimentacoesEstoque>> ListarTodosAsync()
        {
            const string sql = "SELECT * FROM MovimentacoesEstoque";
            using var conexao = new SqlConnection(_connectionString);
            return await conexao.QueryAsync<MovimentacoesEstoque>(sql);
        }

        public async Task<MovimentacoesEstoque?> ConsultarPorIdAsync(long id)
        {
            const string sql = "SELECT * FROM MovimentacoesEstoque WHERE IdMovimentacaoEstoque = @Id";
            using var conexao = new SqlConnection(_connectionString);
            return await conexao.QueryFirstOrDefaultAsync<MovimentacoesEstoque>(sql, new { Id = id });
        }

        public async Task<decimal> ObterQuantidadeDisponivelAsync(long idProduto, long idEstoque)
        {
            // Calcula o saldo somando entradas e subtraindo saídas/vendas/descartes
            // Assumimos que IdTipoMovimentacaoEstoque 1 = Entrada, e os outros são saídas
            const string sql = @"
                SELECT 
                    SUM(CASE WHEN IdTipoMovimentacaoEstoque = 1 THEN Quantidade ELSE -Quantidade END)
                FROM MovimentacoesEstoque
                WHERE IdProduto = @IdProduto AND IdEstoque = @IdEstoque";

            using var conexao = new SqlConnection(_connectionString);
            var saldo = await conexao.ExecuteScalarAsync<decimal?>(sql, new { IdProduto = idProduto, IdEstoque = idEstoque });
            return saldo ?? 0;
        }

        // ── Queries de View (Com JOINs) ───────────────────────────────────────

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
            INNER JOIN Estoque e ON m.IdEstoque = e.IdEstoque
            INNER JOIN Funcionarios fs ON m.IdFuncionarioSolicitador = fs.IdFuncionario
            LEFT JOIN Funcionarios fa ON m.IdFuncionarioAutenticador = fa.IdFuncionario";

        public async Task<IEnumerable<MovimentacaoEstoqueView>> ListarTodosViewAsync()
        {
            using var conexao = new SqlConnection(_connectionString);
            return await conexao.QueryAsync<MovimentacaoEstoqueView>(SQL_BASE_VIEW);
        }

        public async Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorEstoqueAsync(long idEstoque)
        {
            string sql = $"{SQL_BASE_VIEW} WHERE m.IdEstoque = @IdEstoque";
            using var conexao = new SqlConnection(_connectionString);
            return await conexao.QueryAsync<MovimentacaoEstoqueView>(sql, new { IdEstoque = idEstoque });
        }

        public async Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorEstoqueCompletoAsync(long idEstoque) 
            => await ConsultarPorEstoqueAsync(idEstoque);

        public async Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorProdutoAsync(long idProduto)
        {
            string sql = $"{SQL_BASE_VIEW} WHERE m.IdProduto = @IdProduto";
            using var conexao = new SqlConnection(_connectionString);
            return await conexao.QueryAsync<MovimentacaoEstoqueView>(sql, new { IdProduto = idProduto });
        }

        public async Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorProdutoCompletoAsync(long idProduto)
            => await ConsultarPorProdutoAsync(idProduto);

        public async Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorTipoMovimentacaoEstoqueAsync(long idTipo)
        {
            string sql = $"{SQL_BASE_VIEW} WHERE m.IdTipoMovimentacaoEstoque = @IdTipo";
            using var conexao = new SqlConnection(_connectionString);
            return await conexao.QueryAsync<MovimentacaoEstoqueView>(sql, new { IdTipo = idTipo });
        }

        public async Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorFuncionarioSolicitadorAsync(long idFuncionario)
        {
            string sql = $"{SQL_BASE_VIEW} WHERE m.IdFuncionarioSolicitador = @IdFuncionario";
            using var conexao = new SqlConnection(_connectionString);
            return await conexao.QueryAsync<MovimentacaoEstoqueView>(sql, new { IdFuncionario = idFuncionario });
        }

        public async Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorFuncionarioAutenticadorAsync(long idFuncionario)
        {
            string sql = $"{SQL_BASE_VIEW} WHERE m.IdFuncionarioAutenticador = @IdFuncionario";
            using var conexao = new SqlConnection(_connectionString);
            return await conexao.QueryAsync<MovimentacaoEstoqueView>(sql, new { IdFuncionario = idFuncionario });
        }

        public async Task<IEnumerable<MovimentacaoEstoqueView>> ConsultarPorDataAsync(DateTime dataInicio, DateTime dataFim)
        {
            string sql = $"{SQL_BASE_VIEW} WHERE m.DataHora BETWEEN @Inicio AND @Fim";
            using var conexao = new SqlConnection(_connectionString);
            return await conexao.QueryAsync<MovimentacaoEstoqueView>(sql, new { Inicio = dataInicio, Fim = dataFim });
        }

        public async Task<IEnumerable<MovimentacaoEstoqueView>> ListarTodosViewComFiltroAsync(FiltroParaMovimentacaoEstoque filtro)
        {
            var sb = new StringBuilder(SQL_BASE_VIEW);
            sb.Append(" WHERE 1=1");

            var parametros = new DynamicParameters();

            if (filtro.IdEstoque.HasValue)
            {
                sb.Append(" AND m.IdEstoque = @IdEstoque");
                parametros.Add("IdEstoque", filtro.IdEstoque);
            }
            if (filtro.IdProduto.HasValue)
            {
                sb.Append(" AND m.IdProduto = @IdProduto");
                parametros.Add("IdProduto", filtro.IdProduto);
            }
            if (filtro.IdTipoMovimentacao.HasValue)
            {
                sb.Append(" AND m.IdTipoMovimentacaoEstoque = @IdTipo");
                parametros.Add("IdTipo", filtro.IdTipoMovimentacao);
            }
            if (filtro.IdFuncionario.HasValue)
            {
                sb.Append(" AND (m.IdFuncionarioSolicitador = @IdFunc OR m.IdFuncionarioAutenticador = @IdFunc)");
                parametros.Add("IdFunc", filtro.IdFuncionario);
            }
            if (filtro.QuantidadeMinima.HasValue)
            {
                sb.Append(" AND m.Quantidade >= @QtdMin");
                parametros.Add("QtdMin", filtro.QuantidadeMinima);
            }
            if (filtro.QuantidadeMaxima.HasValue)
            {
                sb.Append(" AND m.Quantidade <= @QtdMax");
                parametros.Add("QtdMax", filtro.QuantidadeMaxima);
            }
            if (filtro.DataInicio.HasValue)
            {
                sb.Append(" AND m.DataHora >= @DataIni");
                parametros.Add("DataIni", filtro.DataInicio);
            }
            if (filtro.DataFim.HasValue)
            {
                sb.Append(" AND m.DataHora <= @DataFim");
                parametros.Add("DataFim", filtro.DataFim);
            }

            using var conexao = new SqlConnection(_connectionString);
            return await conexao.QueryAsync<MovimentacaoEstoqueView>(sb.ToString(), parametros);
        }

        // ── Comandos (Escrita) ────────────────────────────────────────────────

        public async Task<MovimentacoesEstoque> CadastrarAsync(MovimentacoesEstoque entidade)
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

        public async Task<MovimentacoesEstoque> AlterarAsync(MovimentacoesEstoque entidade)
        {
            const string sql = @"
                UPDATE MovimentacoesEstoque SET 
                    IdEstoque = @IdEstoque, 
                    IdTipoMovimentacaoEstoque = @IdTipoMovimentacaoEstoque, 
                    IdFuncionarioSolicitador = @IdFuncionarioSolicitador, 
                    IdFuncionarioAutenticador = @IdFuncionarioAutenticador, 
                    IdProduto = @IdProduto, 
                    Quantidade = @Quantidade, 
                    DataHora = @DataHora, 
                    Observacao = @Observacao
                WHERE IdMovimentacaoEstoque = @IdMovimentacaoEstoque";

            using var conexao = new SqlConnection(_connectionString);
            await conexao.ExecuteAsync(sql, entidade);
            return entidade;
        }

        public async Task ExcluirPorIdAsync(long id)
        {
            const string sql = "DELETE FROM MovimentacoesEstoque WHERE IdMovimentacaoEstoque = @Id";
            using var conexao = new SqlConnection(_connectionString);
            await conexao.ExecuteAsync(sql, new { Id = id });
        }

        public async Task ExecutarTransacaoAsync(IEnumerable<MovimentacoesEstoque> movimentacoes)
        {
            using var conexao = new SqlConnection(_connectionString);
            await conexao.OpenAsync();
            using var transacao = conexao.BeginTransaction();

            try
            {
                const string sql = @"
                    INSERT INTO MovimentacoesEstoque 
                    (IdEstoque, IdTipoMovimentacaoEstoque, IdFuncionarioSolicitador, IdFuncionarioAutenticador, IdProduto, Quantidade, DataHora, Observacao)
                    VALUES 
                    (@IdEstoque, @IdTipoMovimentacaoEstoque, @IdFuncionarioSolicitador, @IdFuncionarioAutenticador, @IdProduto, @Quantidade, @DataHora, @Observacao)";

                foreach (var m in movimentacoes)
                {
                    await conexao.ExecuteAsync(sql, m, transaction: transacao);
                }

                transacao.Commit();
            }
            catch (Exception)
            {
                transacao.Rollback();
                throw;
            }
        }
    }
}
