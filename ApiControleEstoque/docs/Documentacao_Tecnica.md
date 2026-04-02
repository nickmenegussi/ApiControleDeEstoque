# Documentação Técnica e Decisões Arquiteturais - API Controle de Estoque

## 1. Visão Geral
Este documento detalha o avanço da estruturação da API de Controle de Estoque construída em .NET 8 com C#. O objetivo primário das últimas atualizações foi implementar os pilares de **Models, Repositories e Controllers** utilizando a micro-ORM **Dapper**.

Lidamos com as seguintes entidades do banco de dados até aqui:
* Produtos
* TiposEstoque
* Fornecedores
* Compras

## 2. Decisões Arquiteturais e Design Patterns

### 2.1 Uso do Padrão Repository
A camada de persistência de dados foi mantida de forma isolada dentro da pasta `Repository` (ex: `ProdutosRepository.cs`). Isso desocupa o papel dos Controladores em conhecer o Banco de Dados. Os Controladores limitam-se a receber as Requições HTTP, repassar a ordem para o Repository e responder usando Códigos de Status HTTP corretos (200 OK, 201 Created, 400 Bad Request, 404 Not Found).

### 2.2 Escolha do Dapper
Efetuamos o mapeamento dos objetos com a extensão **Dapper** (`connection.QueryAsync<T>`). O Dapper oferece uma conversão veloz para objetos nativos e descarta a curva de sobrecarga e processamento mais pesada do Entity Framework, nos permitindo escrever `Queries` flexíveis SQL extraindo a máxima performance.

### 2.3 Tratamento de Exceções de Chave Estrangeira (FK Constraints)
No SQL Server, excluir um registro principal (pai) em que você já possui dependentes vinculados a ele (Exemplo: Deletar um `Fornecedor` que já tenha um histórico de "Compras" reais geradas) devolve a `SqlException n° 547`. Nós implementamos a captura explícita e elegante dessa exceção nos blocos `try/catch` em nossos Controllers (no método DELETE). 
Desta forma, a API amortece e não repassa esse "crash/estouro" em código binário para quem a estiver consumindo, e sim devolve um erro formatado e amigável para o Front-End montar visualizações do tipo: "Para excluir, você precisa excluir os registros vinculados primeiro".

### 2.4 Validações com Annotations e Tipagem Defensiva
Para evitar tráfego de banda inútil ou estouros no banco de dados, configuramos restrições do pacote `System.ComponentModel.DataAnnotations` diretamente em nossos **Models**.
* **`[Required]`**: Bloqueia envios vazios/nulos antes de qualquer contato com os métodos centrais.
* **`[MaxLength(X)]`**: Bloqueia requisições que contenham strings maiores do que a capacidade restrita da nossa Tabela no banco. (Ex: `CodBarras` bloqueado para não estourar os `13` caracteres do tipo `NVARCHAR(13)`, `Nome` travado em `45`, etc).
* Com isso, ativamos o Auto-Validation-Problem (padrão de interface do .NET Core API).

### 2.5 Inserção de Primary Keys (Sem IDENTITY)
Haja vista que a modelagem SQL não registrou colunas em Auto-Incremento (`IDENTITY(1,1)`) para as Chaves Primárias, o banco rejeita se for ordenado Inserir "NULL". Ajustamos todas as Queries de `INSERT` e repositórios para esperarem explicitamente o Repasse Primário do Id em Requests de Criação (POST), impedindo a queda clássica de _'NÃO É POSSÍVEL INSERIR O VALOR NULL NA COLUNA'_.

---

## 3. Configuração de Conexão com o Banco (`appsettings`)

A API monta sua ConnectionString dinamicamente através do injetor do `ConfigurationBuilder` construído no topo dos Repositórios:
Ele tentará acessar prioritariamente o arquivo do ecossistema principal do .NET (o `appsettings.Development.json` no modo de desenvolvimento visual studio) com Fallback para o  `appsettings.json` padrão de publicação fechada.

### 3.1 Modelos e Como Configurar os settings.json
Este snippet (`ConnectionStrings: { DefaultConnection: "..." }`) deve constar dentro do seu arquivo `appsettings.Development.json` bem como no `appsettings.json`:

**Opção A: Conexão Local por Autenticação Windows (Sem Password explícito)**
Bypass direto que puxa a credencial autêntica do seu usuário na Sessão atual do SO Windows. (Ideal na programação local para afastar vazamento de senha em GIT).
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=SUA_MAQUINA;Initial Catalog=trabalhofinaldb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
  }
}
```

**Opção B: Conexão via Autenticação Exclusiva MSSQL (Login e Senha)**
Exige a identificação formal do SA (System Administrator) ou de permissões logicas em ambientes de Nuvem/Produção.
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=SUA_MAQUINA;Initial Catalog=trabalhofinaldb;User ID=administrador;Password=ColoqueASenhaRealAqui;Encrypt=True;TrustServerCertificate=True"
  }
}
```

### 3.2 Quais fatias modificar na Connection String mediante seu PC?
Sempre que baixar a API numa outra máquina ou subir no Azure, troque:
1. **`Data Source=`** -> Nome Instância Servidor (ex: `localhost`, `(localdb)\MSSQLLocalDB`, `SEU-PC-DA-MARCA\SQLEXPRESS`). 
2. **`Initial Catalog=`** -> O nome literal da DataBase do seu trabalho no SQL (ex: `trabalhofinaldb`, `ControleApi`).
3. **`User ID=`** e **`Password=`** -> Caso não passe o comando de "Integrated Security=True", declare o seu usuário real e a sua senha sem aspas sobressalentes.
