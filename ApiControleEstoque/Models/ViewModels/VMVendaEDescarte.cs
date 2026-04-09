using System.ComponentModel.DataAnnotations;

namespace ApiControleEstoque.Models.ViewModels
{
    public class VMVendaEDescarte
    {
        [Required(ErrorMessage = "O Produto é obrigatório.")]
        public long IdProduto { get; set; }

        [Required(ErrorMessage = "O Estoque é obrigatório.")]
        public long IdEstoque { get; set; }

        [Required(ErrorMessage = "A Quantidade é obrigatória.")]
        public int Quantidade { get; set; }

        public long IdFuncionarioSolicitador { get; set; }
        
        public string Observacao { get; set; }
    }
}
