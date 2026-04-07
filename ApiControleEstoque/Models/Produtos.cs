using System.ComponentModel.DataAnnotations;

namespace ApiControleEstoque.Models
{
    public class Produtos
    {
        public long IdProduto { get; set; }

        [Required(ErrorMessage = "O Código de Barras é obrigatório.")]
        [MaxLength(13, ErrorMessage = "O Código de Barras não pode ultrapassar 13 caracteres.")]
        public string CodBarras { get; set; }

        [Required(ErrorMessage = "A Descrição é obrigatória.")]
        [MaxLength(140, ErrorMessage = "A Descrição não pode ultrapassar 140 caracteres.")]
        public string Descricao { get; set; }
    }
}
