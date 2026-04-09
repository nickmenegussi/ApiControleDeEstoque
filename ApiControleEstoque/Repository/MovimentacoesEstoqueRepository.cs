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
    public class MovimentacoesEstoqueRepository
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

        public static async Task<long> CreateAsync(MovimentacoesEstoque movimentacao)
        {
            var query = @"
                INSERT INTO MovimentacoesEstoque (IdEstoque, IdTipoMovimentacaoEstoque, IdFuncionarioSolicitador, IdFuncionarioAutenticador, IdProduto, Quantidade, DataHora)
                VALUES (@IdEstoque, @IdTipoMovimentacaoEstoque, @IdFuncionarioSolicitador, @IdFuncionarioAutenticador, @IdProduto, @Quantidade, @DataHora);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<long>(query, movimentacao);
        }

        public static async Task<int> UpdateAsync(MovimentacoesEstoque movimentacao)
        {
            var query = @"
                UPDATE MovimentacoesEstoque SET 
                    IdEstoque = @IdEstoque, 
                    IdTipoMovimentacaoEstoque = @IdTipoMovimentacaoEstoque, 
                    IdFuncionarioSolicitador = @IdFuncionarioSolicitador, 
                    IdFuncionarioAutenticador = @IdFuncionarioAutenticador, 
                    IdProduto = @IdProduto, 
                    Quantidade = @Quantidade, 
                    DataHora = @DataHora
                WHERE IdMovimentacaoEstoque = @IdMovimentacaoEstoque";

            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(query, movimentacao);
        }

        public static async Task<int> DeleteAsync(long id)
        {
            var query = "DELETE FROM MovimentacoesEstoque WHERE IdMovimentacaoEstoque = @Id";
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(query, new { Id = id });
        }

        public static async Task<decimal> GetQuantidadeDisponivelAsync(long idProduto, long idEstoque)
        {
            // IDs 1 e 4 = Entrada (+), IDs 2 e 3 = Saída (-) conforme README
            var query = @"
                SELECT 
                    SUM(CASE WHEN IdTipoMovimentacaoEstoque IN (1, 4) THEN Quantidade ELSE -Quantidade END)
                FROM MovimentacoesEstoque
                WHERE IdProduto = @IdProduto AND IdEstoque = @IdEstoque";

            using var connection = new SqlConnection(_connectionString);
            var result = await connection.ExecuteScalarAsync<decimal?>(query, new { IdProduto = idProduto, IdEstoque = idEstoque });
            return result ?? 0;
        }

        public static async Task ExecutarVendaOuDescarteAsync(VMVendaEDescarte vm, int idTipo)
        {
            var movimentacao = new MovimentacoesEstoque
            {
                IdEstoque = vm.IdEstoque,
                IdProduto = vm.IdProduto,
                Quantidade = vm.Quantidade,
                IdTipoMovimentacaoEstoque = idTipo,
                IdFuncionarioSolicitador = vm.IdFuncionarioSolicitador,
                DataHora = DateTime.Now
            };

            await CreateAsync(movimentacao);
        }

        public static async Task ExecutarTransferenciaAsync(VMMovimentacoesEstoque vm)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var query = @"
                    INSERT INTO MovimentacoesEstoque (IdEstoque, IdTipoMovimentacaoEstoque, IdFuncionarioSolicitador, IdProduto, Quantidade, DataHora)
                    VALUES (@IdEstoque, @IdTipoMovimentacaoEstoque, @IdFuncionarioSolicitador, @IdProduto, @Quantidade, @DataHora)";

                // Saída da Origem (ID 2 = Saída padrão)
                await connection.ExecuteAsync(query, new { 
                    IdEstoque = vm.IdEstoqueOrigem, 
                    IdTipoMovimentacaoEstoque = 2, 
                    IdFuncionarioSolicitador = vm.IdFuncionarioSolicitador, 
                    IdProduto = vm.IdProduto, 
                    Quantidade = vm.Quantidade, 
                    DataHora = DateTime.Now 
                }, transaction: transaction);

                // Entrada no Destino (ID 1 = Entrada padrão)
                await connection.ExecuteAsync(query, new { 
                    IdEstoque = vm.IdEstoqueDestino, 
                    IdTipoMovimentacaoEstoque = 1, 
                    IdFuncionarioSolicitador = vm.IdFuncionarioSolicitador, 
                    IdProduto = vm.IdProduto, 
                    Quantidade = vm.Quantidade, 
                    DataHora = DateTime.Now 
                }, transaction: transaction);

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
