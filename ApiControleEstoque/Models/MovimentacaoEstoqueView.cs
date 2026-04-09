using System;

namespace ApiControleEstoque.Models
{
    public class MovimentacaoEstoqueView
    {
        public long IdMovimentacaoEstoque { get; set; }
        public decimal Quantidade { get; set; }
        public DateTime DataMovimentacao { get; set; }
        public string TipoMovimentacao { get; set; }
        public string ProdutoNome { get; set; }
        public string EstoqueNome { get; set; }
        public string FuncionarioSolicitador { get; set; }
        public string? FuncionarioAutenticador { get; set; }
        public string? Observacao { get; set; }
    }
}
