using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiControleEstoque.Models
{
    public class Compras
    {
        [Required(ErrorMessage = "A identificação da Compra é obrigatória")]
        public long IdCompra { get; set; }
        
        [Required(ErrorMessage = "A identificação do Fornecedor é obrigatória")]
        public long IdFornecedor { get; set; }
        
        [Required(ErrorMessage = "A identificação do Produto é obrigatória")]
        public long IdProduto { get; set; }
        
        [Required(ErrorMessage = "A Data da compra é obrigatória")]
        public DateTime Data { get; set; }
        
        [Required(ErrorMessage = "A Quantidade da compra é obrigatória")]
        public int Quantidade { get; set; }
    }
}
