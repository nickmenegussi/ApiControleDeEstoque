using System.ComponentModel.DataAnnotations;

namespace ApiControleEstoque.Models
{
    public class Estoques
    {
        public long IdEstoque { get; set; }
        public long IdTipoEstoque { get; set; }

        [Required(ErrorMessage = "A Descrição é obrigatória.")]
        [MaxLength(35, ErrorMessage = "A Descrição não pode ultrapassar 35 caracteres.")]
        public string Descricao { get; set; }

        // Campo auxiliar mapeado nas consultas com JOIN
        public string DescricaoTiposEstoque { get; set; }
    }
}