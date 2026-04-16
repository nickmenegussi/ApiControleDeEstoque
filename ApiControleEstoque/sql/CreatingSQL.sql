CREATE DATABASE trabalhofinaldb;
GO
USE trabalhofinaldb;
GO
-- =============================================
-- TABELAS BASE (sem FKs)
-- =============================================
CREATE TABLE TiposEstoque (
    IdTipoEstoque BIGINT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    Descricao NVARCHAR(35) NOT NULL
);
CREATE TABLE TiposMovimentacaoEstoque (
    IdTipoMovimentacaoEstoque BIGINT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    Descricao NVARCHAR(45) NOT NULL
);
CREATE TABLE Fornecedores (
    IdFornecedor BIGINT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    CNPJ NVARCHAR(18) UNIQUE NOT NULL,
    Nome NVARCHAR(45) NOT NULL
);
CREATE TABLE Produtos (
    IdProduto BIGINT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    CodBarras NVARCHAR(13),
    Descricao NVARCHAR(140) NOT NULL
);
CREATE TABLE Funcionarios (
    IdFuncionario BIGINT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    Nome NVARCHAR(80) NOT NULL,
    Setor NVARCHAR(30),
    Email NVARCHAR(50),
    Senha NVARCHAR(60)
);
-- =============================================
-- TABELAS COM FKs
-- =============================================
CREATE TABLE Estoque (
    IdEstoque BIGINT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    IdTipoEstoque BIGINT NOT NULL,
    Descricao NVARCHAR(35) NOT NULL,
    FOREIGN KEY (IdTipoEstoque) REFERENCES TiposEstoque(IdTipoEstoque)
);
CREATE TABLE Compras (
    IdCompra BIGINT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    IdFornecedor BIGINT NOT NULL,
    IdProduto BIGINT NOT NULL,
    Data SMALLDATETIME NOT NULL,
    Quantidade INT NOT NULL,
    FOREIGN KEY (IdFornecedor) REFERENCES Fornecedores(IdFornecedor),
    FOREIGN KEY (IdProduto) REFERENCES Produtos(IdProduto)
);
CREATE TABLE MovimentacoesEstoque (
    IdMovimentacaoEstoque BIGINT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    IdEstoque BIGINT NOT NULL,
    IdTipoMovimentacaoEstoque BIGINT NOT NULL,
    IdFuncionarioSolicitador BIGINT NOT NULL,
    IdFuncionarioAutenticador BIGINT NULL,
    IdProduto BIGINT NOT NULL,
    Quantidade BIGINT NOT NULL,
    DataHora SMALLDATETIME NOT NULL,
    Observacao NVARCHAR(255) NULL,
    FOREIGN KEY (IdEstoque) REFERENCES Estoque(IdEstoque),
    FOREIGN KEY (IdTipoMovimentacaoEstoque) REFERENCES TiposMovimentacaoEstoque(IdTipoMovimentacaoEstoque),
    FOREIGN KEY (IdFuncionarioSolicitador) REFERENCES Funcionarios(IdFuncionario),
    FOREIGN KEY (IdFuncionarioAutenticador) REFERENCES Funcionarios(IdFuncionario),
    FOREIGN KEY (IdProduto) REFERENCES Produtos(IdProduto)
);