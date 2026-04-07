using System;
using System.ComponentModel.DataAnnotations;

namespace ApiControleEstoque.Models
{
    public class MovimentacoesEstoque
    {
        public long IdMovimentacaoEstoque { get; set; }

        [Required(ErrorMessage = "O Estoque é obrigatório.")]
        public long IdEstoque { get; set; }

        [Required(ErrorMessage = "O Tipo de Movimentação é obrigatório.")]
        public long IdTipoMovimentacaoEstoque { get; set; }

        [Required(ErrorMessage = "O Funcionário Solicitador é obrigatório.")]
        public long IdFuncionarioSolicitador { get; set; }

        public long? IdFuncionarioAutenticador { get; set; } // nullable, pois pode ainda não ter sido autenticada

        [Required(ErrorMessage = "O Produto é obrigatório.")]
        public long IdProduto { get; set; }

        [Required(ErrorMessage = "A Quantidade é obrigatória.")]
        [Range(1, int.MaxValue, ErrorMessage = "A Quantidade deve ser maior que zero.")]
        public int Quantidade { get; set; }

        [Required(ErrorMessage = "A Data/Hora é obrigatória.")]
        public DateTime DataHora { get; set; }
    }
}
