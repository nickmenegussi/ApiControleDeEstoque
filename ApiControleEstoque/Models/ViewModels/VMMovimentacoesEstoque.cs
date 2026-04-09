using System.ComponentModel.DataAnnotations;

namespace ApiControleEstoque.Models.ViewModels
{
    public class VMMovimentacoesEstoque
    {
        [Required(ErrorMessage = "O Produto é obrigatório.")]
        public long IdProduto { get; set; }

        [Required(ErrorMessage = "O Estoque de Origem é obrigatório.")]
        public long IdEstoqueOrigem { get; set; }

        [Required(ErrorMessage = "O Estoque de Destino é obrigatório.")]
        public long IdEstoqueDestino { get; set; }

        [Required(ErrorMessage = "A Quantidade é obrigatória.")]
        public int Quantidade { get; set; }

        public long IdFuncionarioSolicitador { get; set; }

        public string Observacao { get; set; }
    }
}
