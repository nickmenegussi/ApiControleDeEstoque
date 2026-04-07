namespace ApiControleEstoque.Models.ViewModels
{
    public class EstoquesViewModel
    {
        public long IdEstoque { get; set; }
        public long IdTipoEstoque { get; set; }
        public string Descricao { get; set; }

        // Campo auxiliar (Join)
        public string DescricaoTiposEstoque { get; set; }
    }
}
