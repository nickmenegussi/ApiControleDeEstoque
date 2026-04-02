using System.ComponentModel.DataAnnotations;

namespace ApiControleEstoque.Models
{
    public class Produto
    {
        [Required(ErrorMessage = "A identificação do Produto é obrigatória")]
        public long IdProduto { get; set; }
        
        [Required(ErrorMessage = "O Código de Barras é obrigatório")]
        [MaxLength(13, ErrorMessage = "O Código de Barras não pode ultrapassar 13 caracteres.")]
        public string CodBarras { get; set; }
        
        [Required(ErrorMessage = "A Descrição do produto é obrigatória")]
        [MaxLength(140, ErrorMessage = "A Descrição do Produto não pode ultrapassar 140 caracteres.")]
        public string Descricao { get; set; }
    }
}
