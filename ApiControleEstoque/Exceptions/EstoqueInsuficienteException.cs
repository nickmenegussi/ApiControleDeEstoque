using System;

namespace ApiControleEstoque.Exceptions
{
    public class EstoqueInsuficienteException : Exception
    {
        public EstoqueInsuficienteException() : base("Quantidade em estoque insuficiente para realizar a movimentação.") { }
        public EstoqueInsuficienteException(string message) : base(message) { }
    }
}
