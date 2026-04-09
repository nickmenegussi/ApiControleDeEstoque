using System;
using System.ComponentModel.DataAnnotations;

namespace ApiControleEstoque.Models
{
    public class MovimentacoesEstoque
    {
        [Key]
        public long IdMovimentacaoEstoque { get; set; }

        [Required]
        public long? IdEstoque { get; set; }

        [Required]
        public long? IdTipoMovimentacaoEstoque { get; set; }

        [Required]
        public long? IdFuncionarioSolicitador { get; set; }

        public long? IdFuncionarioAutenticador { get; set; }

        [Required]
        public long? IdProduto { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int? Quantidade { get; set; }

        [Required]
        public DateTime? DataHora { get; set; }

        public string? Observacao { get; set; }
    }
}
