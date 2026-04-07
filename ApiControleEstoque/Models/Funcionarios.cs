using System.ComponentModel.DataAnnotations;

namespace ApiControleEstoque.Models
{
    public class Funcionarios
    {
        public long IdFuncionario { get; set; }

        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [MaxLength(80, ErrorMessage = "O Nome não pode ultrapassar 80 caracteres.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O Setor é obrigatório.")]
        [MaxLength(30, ErrorMessage = "O Setor não pode ultrapassar 30 caracteres.")]
        public string Setor { get; set; }

        [Required(ErrorMessage = "O E-mail é obrigatório.")]
        [MaxLength(50, ErrorMessage = "O E-mail não pode ultrapassar 50 caracteres.")]
        public string Email { get; set; }

        [MaxLength(60, ErrorMessage = "A Senha não pode ultrapassar 60 caracteres.")]
        public string Senha { get; set; }
    }
}
