using System;

namespace ApiControleEstoque.Models.ViewModels
{
    public class ProdutoMovimentacaoRecenteViewModel
    {
        public long IdMovimentacaoEstoque { get; set; }
        public int Quantidade { get; set; }
        public DateTime DataHora { get; set; }
        public string TipoMovimentacao { get; set; }
        public string EstoqueNome { get; set; }
        public string SolicitadorNome { get; set; }
        public string? AutenticadorNome { get; set; }
    }
}
