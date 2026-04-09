using System.ComponentModel.DataAnnotations;

namespace ApiControleEstoque.Models
{
    public class PesquisaPadrao
    {
        [Required(ErrorMessage = "O termo de pesquisa é obrigatório.")]
        public string Query { get; set; }
    }

    public class PesquisaDescricaoCodBarra
    {
        public string Descricao { get; set; }
        public string CodBarra { get; set; }
    }

    public class PesquisaFuncionario
    {
        public long? Id { get; set; }
        public string Nome { get; set; }
        public string Setor { get; set; }
        public string Email { get; set; }
    }

    public class FiltroParaCompra
    {
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int? QuantidadeMin { get; set; }
        public long? IdProduto { get; set; }
        public long? IdFornecedor { get; set; }
    }

    public class SolicitacaoCompra
    {
        [Required(ErrorMessage = "O Fornecedor é obrigatório.")]
        public long IdFornecedor { get; set; }

        [Required(ErrorMessage = "O Produto é obrigatório.")]
        public long IdProduto { get; set; }

        [Required(ErrorMessage = "O Funcionário é obrigatório.")]
        public long IdFuncionario { get; set; }

        [Required(ErrorMessage = "A Quantidade é obrigatória.")]
        [Range(1, int.MaxValue, ErrorMessage = "A Quantidade deve ser maior que zero.")]
        public int Quantidade { get; set; }

        [Required(ErrorMessage = "A Data é obrigatória.")]
        public DateTime Data { get; set; }
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "O Email do Funcionário é obrigatório.")]

        public string Email { get; set; }

        [Required(ErrorMessage = "A Senha é obrigatória.")]
        public string Senha { get; set; } = string.Empty;
    }

    public class AlterarSenhaRequest
    {
        [Required(ErrorMessage = "O ID do Funcionário é obrigatório.")]
        [Range(1, long.MaxValue, ErrorMessage = "ID inválido. O ID deve ser maior que zero.")]
        public long IdFuncionario { get; set; }

        [Required(ErrorMessage = "A Senha atual é obrigatória.")]
        public string SenhaAtual { get; set; } = string.Empty;

        [Required(ErrorMessage = "A Nova Senha é obrigatória.")]
        public string NovaSenha { get; set; } = string.Empty;
    }

    public class PesquisaEstoque
    {
        public long IdTipoEstoque { get; set; }
        public string Descricao { get; set; }
    }

    public class FiltroParaMovimentacaoEstoque
    {
        public long?     IdEstoque          { get; set; }
        public long?     IdProduto          { get; set; }
        public int?      IdTipoMovimentacao { get; set; }
        public DateTime? DataInicio         { get; set; }
        public DateTime? DataFim            { get; set; }
    }

    public class TransferenciaEstoqueRequest
    {
        [Required(ErrorMessage = "O Produto é obrigatório.")]
        public long? IdProduto { get; set; }

        [Required(ErrorMessage = "O Estoque de Origem é obrigatório.")]
        public long? IdEstoqueOrigem { get; set; }

        [Required(ErrorMessage = "O Estoque de Destino é obrigatório.")]
        public long? IdEstoqueDestino { get; set; }

        [Required(ErrorMessage = "A Quantidade é obrigatória.")]
        public int? Quantidade { get; set; }

        public long IdFuncionarioSolicitador { get; set; }
    }

    public class MovimentacaoProdutoRequest
    {
        [Required(ErrorMessage = "O Produto é obrigatório.")]
        public long? IdProduto { get; set; }

        [Required(ErrorMessage = "O Estoque é obrigatório.")]
        public long? IdEstoque { get; set; }

        [Required(ErrorMessage = "A Quantidade é obrigatória.")]
        public int? Quantidade { get; set; }

        public long IdFuncionarioSolicitador { get; set; }
        
        public string? Observacao { get; set; }
    }
}
