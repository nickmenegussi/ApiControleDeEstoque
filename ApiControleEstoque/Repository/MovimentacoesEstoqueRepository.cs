using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using ApiControleEstoque.Models;
using ApiControleEstoque.Models.ViewModels;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ApiControleEstoque.Repository
{
    public static class MovimentacoesEstoqueRepository
    {
        private static readonly string _connectionString;

        static MovimentacoesEstoqueRepository()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        }

        public static async Task<List<MovimentacoesEstoqueViewModel>> GetAllMovimentacoesAsync()
        {
            var query = @"
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

            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<MovimentacoesEstoqueViewModel>(query);
            return list.AsList();
        }

        public static async Task<MovimentacoesEstoque?> GetByIdMovimentacaoAsync(long id)
        {
            if (id <= 0) return null;
            var query = "SELECT * FROM MovimentacoesEstoque WHERE IdMovimentacaoEstoque = @id";
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<MovimentacoesEstoque>(query, new { id });
        }

        public static async Task<List<MovimentacoesEstoqueViewModel>> GetMovimentacoesByEstoqueAsync(long idEstoque)
        {
            if (idEstoque <= 0) return new List<MovimentacoesEstoqueViewModel>();
            var query = @"
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
                LEFT JOIN Funcionarios fa ON m.IdFuncionarioAutenticador = fa.IdFuncionario
                WHERE m.IdEstoque = @idEstoque";

            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<MovimentacoesEstoqueViewModel>(query, new { idEstoque });
            return list.AsList();
        }

        public static async Task<List<MovimentacoesEstoqueViewModel>> GetMovimentacoesByProdutoAsync(long idProduto)
        {
            if (idProduto <= 0) return new List<MovimentacoesEstoqueViewModel>();
            var query = @"
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
                LEFT JOIN Funcionarios fa ON m.IdFuncionarioAutenticador = fa.IdFuncionario
                WHERE m.IdProduto = @idProduto";

            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<MovimentacoesEstoqueViewModel>(query, new { idProduto });
            return list.AsList();
        }

        public static async Task<List<MovimentacoesEstoqueViewModel>> GetMovimentacoesByPeriodoAsync(DateTime inicio, DateTime fim)
        {
            var query = @"
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
                LEFT JOIN Funcionarios fa ON m.IdFuncionarioAutenticador = fa.IdFuncionario
                WHERE m.DataHora BETWEEN @inicio AND @fim";

            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<MovimentacoesEstoqueViewModel>(query, new { inicio, fim });
            return list.AsList();
        }

        public static async Task<List<MovimentacoesEstoqueViewModel>> ConsultarPorTudoAsync(FiltroParaMovimentacaoEstoque filtro)
        {
            var query = @"
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
                LEFT JOIN Funcionarios fa ON m.IdFuncionarioAutenticador = fa.IdFuncionario
                WHERE (@DataInicio IS NULL OR m.DataHora >= @DataInicio)
                  AND (@DataFim IS NULL OR m.DataHora <= @DataFim)
                  AND (@IdProduto IS NULL OR m.IdProduto = @IdProduto)
                  AND (@IdEstoque IS NULL OR m.IdEstoque = @IdEstoque)
                  AND (@IdTipoMovimentacao IS NULL OR m.IdTipoMovimentacaoEstoque = @IdTipoMovimentacao)";
            
            using var connection = new SqlConnection(_connectionString);
            var list = await connection.QueryAsync<MovimentacoesEstoqueViewModel>(query, new { 
                DataInicio = filtro.DataInicio, 
                DataFim = filtro.DataFim,
                IdProduto = filtro.IdProduto,
                IdEstoque = filtro.IdEstoque,
                IdTipoMovimentacao = filtro.IdTipoMovimentacao
            });
            return list.AsList();
        }

        public static async Task<int> CreateMovimentacaoAsync(MovimentacoesEstoque movimentacao)
        {
            if (movimentacao.IdEstoque <= 0 || movimentacao.IdProduto <= 0 || movimentacao.Quantidade <= 0) return -1;

            var query = @"
                INSERT INTO MovimentacoesEstoque 
                (IdEstoque, IdTipoMovimentacaoEstoque, IdFuncionarioSolicitador, IdFuncionarioAutenticador, IdProduto, Quantidade, DataHora, Observacao)
                VALUES 
                (@IdEstoque, @IdTipoMovimentacaoEstoque, @IdFuncionarioSolicitador, @IdFuncionarioAutenticador, @IdProduto, @Quantidade, @DataHora, @Observacao)";

            using var connection = new SqlConnection(_connectionString);
            try
            {
                return await connection.ExecuteAsync(query, movimentacao);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) return -2;
                throw;
            }
        }

        public static async Task<int> UpdateMovimentacaoAsync(MovimentacoesEstoque movimentacao)
        {
            if (movimentacao.IdMovimentacaoEstoque <= 0 || movimentacao.Quantidade <= 0) return -1;

            var query = @"
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

            using var connection = new SqlConnection(_connectionString);
            try
            {
                return await connection.ExecuteAsync(query, movimentacao);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) return -2;
                throw;
            }
        }

        public static async Task<int> DeleteMovimentacaoAsync(long id)
        {
            if (id <= 0) return 0;
            var query = "DELETE FROM MovimentacoesEstoque WHERE IdMovimentacaoEstoque = @Id";
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(query, new { Id = id });
        }

        public static async Task<long> ObterQuantidadeDisponivelAsync(long idProduto, long idEstoque)
        {
            var query = @"
                SELECT 
                    ISNULL(SUM(CASE WHEN IdTipoMovimentacaoEstoque IN (1, 4) THEN Quantidade ELSE 0 END), 0) - 
                    ISNULL(SUM(CASE WHEN IdTipoMovimentacaoEstoque IN (2, 3) THEN Quantidade ELSE 0 END), 0)
                FROM MovimentacoesEstoque
                WHERE IdProduto = @idProduto AND IdEstoque = @idEstoque";

            using var connection = new SqlConnection(_connectionString);
            return (long)(await connection.ExecuteScalarAsync<decimal?>(query, new { idProduto, idEstoque }) ?? 0);
        }

        public static async Task<int> VenderOuDescartarAsync(MovimentacaoProdutoRequest request, int idTipoEspecifico)
        {
            if (request.IdProduto == null || request.IdEstoque == null || request.Quantidade == null || request.Quantidade <= 0) return -1;
            
            var qtdDisp = await ObterQuantidadeDisponivelAsync(request.IdProduto.Value, request.IdEstoque.Value);
            if (request.Quantidade > qtdDisp) return -2; // Saldo insuficiente

            var query = @"
                INSERT INTO MovimentacoesEstoque 
                (IdEstoque, IdTipoMovimentacaoEstoque, IdFuncionarioSolicitador, IdProduto, Quantidade, DataHora, Observacao)
                VALUES 
                (@IdEstoque, @IdTipoMovimentacaoEstoque, @IdFuncionarioSolicitador, @IdProduto, @Quantidade, @DataHora, @Observacao)";

            using var connection = new SqlConnection(_connectionString);
            try
            {
                return await connection.ExecuteAsync(query, new {
                    IdEstoque = request.IdEstoque.Value,
                    IdTipoMovimentacaoEstoque = idTipoEspecifico,
                    IdFuncionarioSolicitador = request.IdFuncionarioSolicitador,
                    IdProduto = request.IdProduto.Value,
                    Quantidade = request.Quantidade.Value,
                    DataHora = DateTime.Now,
                    Observacao = request.Observacao ?? ""
                });
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) return -3;
                throw;
            }
        }

        public static async Task<int> TransferirAsync(TransferenciaEstoqueRequest request)
        {
            if (request.IdProduto == null || request.IdEstoqueOrigem == null || request.IdEstoqueDestino == null || request.Quantidade == null || request.Quantidade <= 0) return -1;
            if (request.IdEstoqueOrigem == request.IdEstoqueDestino) return -3;

            var qtdDisp = await ObterQuantidadeDisponivelAsync(request.IdProduto.Value, request.IdEstoqueOrigem.Value);
            if (request.Quantidade > qtdDisp) return -2; // Saldo insuficiente

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var query = @"
                    INSERT INTO MovimentacoesEstoque (IdEstoque, IdTipoMovimentacaoEstoque, IdFuncionarioSolicitador, IdProduto, Quantidade, DataHora, Observacao)
                    VALUES (@IdEstoque, @IdTipoMovimentacaoEstoque, @IdFuncionarioSolicitador, @IdProduto, @Quantidade, @DataHora, @Observacao)";

                // Saída (ID 2 = Saída Padrão)
                await connection.ExecuteAsync(query, new { 
                    IdEstoque = request.IdEstoqueOrigem, 
                    IdTipoMovimentacaoEstoque = 2, 
                    IdFuncionarioSolicitador = request.IdFuncionarioSolicitador, 
                    IdProduto = request.IdProduto, 
                    Quantidade = request.Quantidade, 
                    DataHora = DateTime.Now,
                    Observacao = $"Transferência para {request.IdEstoqueDestino}."
                }, transaction: transaction);

                // Entrada (ID 1 = Entrada Padrão)
                await connection.ExecuteAsync(query, new { 
                    IdEstoque = request.IdEstoqueDestino, 
                    IdTipoMovimentacaoEstoque = 1, 
                    IdFuncionarioSolicitador = request.IdFuncionarioSolicitador, 
                    IdProduto = request.IdProduto, 
                    Quantidade = request.Quantidade, 
                    DataHora = DateTime.Now,
                    Observacao = $"Transferência de {request.IdEstoqueOrigem}."
                }, transaction: transaction);

                await transaction.CommitAsync();
                return 1;
            }
            catch (SqlException ex)
            {
                await transaction.RollbackAsync();
                if (ex.Number == 547) return -4; // Constraint FK
                throw;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
