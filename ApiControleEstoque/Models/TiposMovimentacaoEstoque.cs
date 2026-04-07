using System.ComponentModel.DataAnnotations;

namespace ApiControleEstoque.Models
{
    public class TiposMovimentacaoEstoque
    {
        public long IdTipoMovimentacaoEstoque { get; set; }

        [Required(ErrorMessage = "A Descrição é obrigatória.")]
        [MaxLength(45, ErrorMessage = "A Descrição não pode ultrapassar 45 caracteres.")]
        public string Descricao { get; set; }
    }
}
