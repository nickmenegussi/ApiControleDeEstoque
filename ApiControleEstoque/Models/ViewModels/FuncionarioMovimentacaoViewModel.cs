using System;

namespace ApiControleEstoque.Models.ViewModels
{
    public class FuncionarioMovimentacaoViewModel
    {
        public long IdMovimentacaoEstoque { get; set; }
        public int Quantidade { get; set; }
        public DateTime DataHora { get; set; }
        public string Produto { get; set; }
        public string Estoque { get; set; }
        public string TipoMovimentacao { get; set; }
    }
}
