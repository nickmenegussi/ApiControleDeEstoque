using System;
using System.ComponentModel.DataAnnotations;

namespace ApiControleEstoque.Models.ViewModels
{
    /// <summary>ViewModel para operações de venda e descarte de produto.</summary>
    public class VMVendaEDescarte
    {
        [Required]
        [Range(1, long.MaxValue)]
        public long? IdProduto { get; set; }

        [Required]
        [Range(1, long.MaxValue)]
        public long? IdEstoque { get; set; }

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
