using System;

namespace ApiControleEstoque.Models.ViewModels
{
    /// <summary>Filtros combinados para pesquisa avançada de movimentações.</summary>
    public class FiltroParaMovimentacaoEstoque
    {
        public long?     IdEstoque          { get; set; }
        public long?     IdProduto          { get; set; }
        public long?     IdTipoMovimentacao { get; set; }
        public long?     IdFuncionario      { get; set; }
        public decimal?  QuantidadeMinima   { get; set; }
        public decimal?  QuantidadeMaxima   { get; set; }
        public DateTime? DataInicio         { get; set; }
        public DateTime? DataFim            { get; set; }
    }
}
