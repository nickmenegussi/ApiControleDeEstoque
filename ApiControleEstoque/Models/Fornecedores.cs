using System.ComponentModel.DataAnnotations;

namespace ApiControleEstoque.Models
{
    public class Fornecedores
    {
        [Required(ErrorMessage = "A identificação do Fornecedor é obrigatória")]
        public long IdFornecedor { get; set; }
        
        [Required(ErrorMessage = "O CNPJ do fornecedor é obrigatório")]
        [MaxLength(18, ErrorMessage = "O CNPJ não pode ultrapassar 18 caracteres.")]
        public string CNPJ { get; set; }
        
        [Required(ErrorMessage = "O Nome do fornecedor é obrigatório")]
        [MaxLength(45, ErrorMessage = "O Nome do fornecedor não pode ultrapassar 45 caracteres.")]
        public string Nome { get; set; }
    }
}
