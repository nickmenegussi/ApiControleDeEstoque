using System.ComponentModel.DataAnnotations;

namespace ApiControleEstoque.Models
{
    public class Fornecedores
    {
        public long IdFornecedor { get; set; }

        [Required(ErrorMessage = "O CNPJ é obrigatório.")]
        [MaxLength(18, ErrorMessage = "O CNPJ não pode ultrapassar 18 caracteres.")]
        public string CNPJ { get; set; }

        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [MaxLength(45, ErrorMessage = "O Nome não pode ultrapassar 45 caracteres.")]
        public string Nome { get; set; }

        //public ICollection<Compras> Compras { get; set; }
    }
}
