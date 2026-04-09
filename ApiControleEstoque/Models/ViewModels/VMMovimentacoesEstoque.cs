using System;
using System.ComponentModel.DataAnnotations;

namespace ApiControleEstoque.Models.ViewModels
{
    /// <summary>ViewModel para transferência de produto entre estoques.</summary>
    public class VMMovimentacoesEstoque
    {
        [Required]
        [Range(1, long.MaxValue)]
        public long? IdProduto { get; set; }

        [Required]
        [Range(1, long.MaxValue)]
        public long? IdEstoqueOrigem { get; set; }

        [Required]
        [Range(1, long.MaxValue)]
        public long? IdEstoqueDestino { get; set; }

        [Required]
        [Range(0.001, double.MaxValue)]
        public decimal? Quantidade { get; set; }

        [Required]
        [Range(1, long.MaxValue)]
        public long? IdFuncionarioSolicitador { get; set; }

        [MaxLength(500)]
        public string? Observacao { get; set; }
    }
}
