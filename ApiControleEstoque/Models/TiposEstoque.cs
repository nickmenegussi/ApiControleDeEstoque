using System.ComponentModel.DataAnnotations;

namespace ApiControleEstoque.Models
{
    public class TiposEstoque
    {
        [Required(ErrorMessage = "A identificação do Tipo de Estoque é obrigatória")]
        public long IdTipoEstoque { get; set; }
        
        [Required(ErrorMessage = "A Descrição do Tipo de Estoque é obrigatória")]
        [MaxLength(35, ErrorMessage = "A Descrição não pode ultrapassar 35 caracteres.")]
        public string Descricao { get; set; }
    }
}
