using ApiControleEstoque.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiControleEstoque.Repository
{
    public class ComprasRepository
    {
        private static readonly string _connectionString;

        static ComprasRepository()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        }

        public static async Task<List<Compras>> GetAllComprasAsync()
        {
            var query = @"
                SELECT 
                    c.IdCompra,
                    c.IdFornecedor,
                    c.IdProduto,
                    c.Data,
                    c.Quantidade,
                    f.Nome AS FornecedorNome,
                    p.Descricao AS ProdutoDescricao,
                    p.CodBarras AS CodBarras
                FROM Compras c
                LEFT JOIN Fornecedores f ON c.IdFornecedor = f.IdFornecedor
                LEFT JOIN Produtos p ON c.IdProduto = p.IdProduto";
            using var connection = new SqlConnection(_connectionString);
            var listCompras = await connection.QueryAsync<Compras>(query);
            return listCompras.AsList();
        }

        public static async Task<Compras?> GetByIdComprasAsync(long idCompra)
        {
            var query = @"
                SELECT 
                    c.IdCompra,
                    c.IdFornecedor,
                    c.IdProduto,
                    c.Data,
                    c.Quantidade,
                    f.Nome AS FornecedorNome,
                    p.Descricao AS ProdutoDescricao,
                    p.CodBarras AS CodBarras
                FROM Compras as c
                LEFT JOIN Fornecedores f ON c.IdFornecedor = f.IdFornecedor
                LEFT JOIN Produtos p ON c.IdProduto = p.IdProduto
                WHERE c.IdCompra = @IdCompra";
            using var connection = new SqlConnection(_connectionString);
            var compras = await connection.QueryFirstOrDefaultAsync<Compras>(query, new { IdCompra = idCompra });
            return compras;
        }

        public static async Task<List<Compras>> GetComprasByFornecedorAsync(long idFornecedor)
        {
            var query = @"
                SELECT 
                    c.IdCompra,
                    c.IdFornecedor,
                    c.IdProduto,
                    c.Data,
                    c.Quantidade,
                    f.Nome AS FornecedorNome,
                    p.Descricao AS ProdutoDescricao,
                    p.CodBarras AS CodBarras
                FROM Compras c
                LEFT JOIN Fornecedores f ON c.IdFornecedor = f.IdFornecedor
                LEFT JOIN Produtos p ON c.IdProduto = p.IdProduto
                WHERE c.IdFornecedor = @IdFornecedor";

            using var connection = new SqlConnection(_connectionString);
            var compras = await connection.QueryAsync<Compras>(query, new { IdFornecedor = idFornecedor });
            return compras.AsList();
        }

        public static async Task<List<Compras>> GetComprasByProdutoIdAsync(long idProduto)
        {
            var query = @"
                SELECT 
                    c.IdCompra,
                    c.IdFornecedor,
                    c.IdProduto,
                    c.Data,
                    c.Quantidade,
                    f.Nome AS FornecedorNome,
                    p.Descricao AS ProdutoDescricao,
                    p.CodBarras AS CodBarras
                FROM Compras c
                LEFT JOIN Fornecedores f ON c.IdFornecedor = f.IdFornecedor
                LEFT JOIN Produtos p ON c.IdProduto = p.IdProduto
                WHERE c.IdProduto = @IdProduto";

            using var connection = new SqlConnection(_connectionString);
            var compras = await connection.QueryAsync<Compras>(query, new { IdProduto = idProduto });
            return compras.AsList();
        }

        public static async Task<List<Compras>> GetComprasByPeriodoAsync(DateTime inicio, DateTime fim)
        {
            var query = @"
                SELECT 
                    c.IdCompra,
                    c.IdFornecedor,
                    c.IdProduto,
                    c.Data,
                    c.Quantidade,
                    f.Nome AS FornecedorNome,
                    p.Descricao AS ProdutoDescricao,
                    p.CodBarras AS CodBarras
                FROM Compras c
                LEFT JOIN Fornecedores f ON c.IdFornecedor = f.IdFornecedor
                LEFT JOIN Produtos p ON c.IdProduto = p.IdProduto
                WHERE c.Data BETWEEN @Inicio AND @Fim";
            using var connection = new SqlConnection(_connectionString);
            var compras = await connection.QueryAsync<Compras>(query, new { Inicio = inicio, Fim = fim });
            return compras.AsList();
        }

        public static async Task<int> CreateCompraAsync(Compras compra)
        {
            if (compra.Quantidade <= 0)
            {
                return -1;
            }

            var query = @"
                INSERT INTO Compras (IdCompra, IdFornecedor, IdProduto, Data, Quantidade)
                VALUES (@IdCompra, @IdFornecedor, @IdProduto, @Data, @Quantidade)
            ";

            using var connection = new SqlConnection(_connectionString);
            try
            {
                var rowsAffected = await connection.ExecuteAsync(query, compra);
                return rowsAffected;
            }
            catch (SqlException ex)
            {
                // Se der erro de FK por Produto ou Fornecedor inexistente.
                if (ex.Number == 547) return -2;
                throw;
            }
        }

        public static async Task<int> UpdateComprasAsync(Compras compra)
        {
            if (compra.Quantidade <= 0)
            {
                return -1;
            }

            var query = @"
                UPDATE Compras
                SET IdFornecedor = @IdFornecedor,
                    IdProduto = @IdProduto,
                    Data = @Data,
                    Quantidade = @Quantidade
                WHERE IdCompra = @IdCompra";

            using var connection = new SqlConnection(_connectionString);
            try
            {
                var affectedRows = await connection.ExecuteAsync(query, compra);
                return affectedRows;
            }
            catch (SqlException ex)
            {
                // Se der erro de FK por Produto ou Fornecedor inexistente.
                if (ex.Number == 547) return -2;
                throw;
            }
        }

        public static async Task<int> DeleteComprasAsync(long idCompra)
        {
            var query = "DELETE FROM Compras WHERE IdCompra = @IdCompra";
            using var connection = new SqlConnection(_connectionString);
            var affectedRows = await connection.ExecuteAsync(query, new { IdCompra = idCompra });
            return affectedRows;
        }
    }
}
