using System;

namespace ApiControleEstoque.Models.ViewModels
{
    public class ComprasViewModel
    {
        public long IdCompra { get; set; }
        public long IdFornecedor { get; set; }
        public long IdProduto { get; set; }
        public DateTime Data { get; set; }
        public int Quantidade { get; set; }

        // Campos auxiliares (Joins)
        public string FornecedorNome { get; set; }
        public string ProdutoDescricao { get; set; }
        public string CodBarras { get; set; }
    }
}
