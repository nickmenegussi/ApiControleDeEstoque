using System;

namespace ApiControleEstoque.Exceptions
{
    public class EntidadeNaoEncontradaException : Exception
    {
        public EntidadeNaoEncontradaException() : base("A entidade solicitada não foi encontrada.") { }
        public EntidadeNaoEncontradaException(string message) : base(message) { }
    }
}
