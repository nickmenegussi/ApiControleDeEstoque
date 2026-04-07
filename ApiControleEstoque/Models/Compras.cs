using System;
using System.ComponentModel.DataAnnotations;

namespace ApiControleEstoque.Models
{
    public class Compras
    {
        public long IdCompra { get; set; }

        [Required(ErrorMessage = "O Fornecedor é obrigatório.")]
        public long IdFornecedor { get; set; }

        [Required(ErrorMessage = "O Produto é obrigatório.")]
        public long IdProduto { get; set; }

        [Required(ErrorMessage = "A Data é obrigatória.")]
        public DateTime Data { get; set; }

        [Required(ErrorMessage = "A Quantidade é obrigatória.")]
        [Range(1, int.MaxValue, ErrorMessage = "A Quantidade deve ser maior que zero.")]
        public int Quantidade { get; set; }
    }
}
