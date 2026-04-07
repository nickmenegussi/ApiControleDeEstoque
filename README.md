# 📦 Sistema de Controle de Estoque (API) - Gestão de Moda

Este projeto é uma Web API de alto desempenho desenvolvida em **ASP.NET Core 8** e **Dapper**, projetada especificamente para o setor de moda (roupas, calçados e acessórios). 

O sistema utiliza uma arquitetura de Repositórios Estáticos e SQL otimizado para lidar com múltiplos depósitos e grandes volumes de movimentação.

---

## 🛠️ Tecnologias Utilizadas
- **Core:** .NET 8 (C#)
- **Acesso a Dados:** Dapper (Micro-ORM para máxima performance)
- **Banco de Dados:** SQL Server (Microsoft.Data.SqlClient)
- **Documentação:** Swagger / OpenAPI

---

## 📖 Guia de Módulos e Inteligência do Sistema

Adicionamos funcionalidades avançadas que transformam o sistema em um ERP robusto. Abaixo está a explicação do **"Porquê"** de cada recurso especial:

### 1. Módulo de Produtos (`/api/Produtos`)
*   **Busca Dinâmica (`POST /tudo`)**: 
    *   *Por que?* Centraliza a pesquisa em uma única caixa de texto que entende IDs, Descrições e Códigos de Barra simultaneamente. Muito mais rápido para o usuário no dia a dia.
*   **Saldo por Estoque (`GET /quantPorEstoque/{id}`)**: 
    *   *Por que?* Em vez de ver apenas "Tenho 100 camisetas", você vê "Tenho 50 no Almoxarifado A e 50 na Loja B". Essencial para logística multidepósito.

### 2. Módulo de Compras (`/api/Compras`) - **OPERAÇÃO MESTRE**
*   **Compra com Entrada Automática (`POST /Compra`)**: 
    *   *Por que?* Ao registrar uma compra, o sistema utiliza uma **Transação SQL** para dar entrada física no estoque ao mesmo tempo. Isso evita erros humanos de esquecer de atualizar o saldo e garante que o estoque nunca fique "furado".
*   **Filtros Avançados (`POST /tudo`)**: 
    *   *Por que?* Permite gerar relatórios complexos cruzando Fornecedor + Data + Produto sem a necessidade de múltiplas telas.

### 3. Módulo de Funcionários (`/api/Funcionarios`)
*   **Rastreabilidade de Operações (`GET /movimentacoesFuncionario/{id}`)**: 
    *   *Por que?* Permite auditar quem deu entrada ou saída em cada peça de roupa, garantindo segurança e controle de responsabilidades.
*   **Busca Inteligente (`POST /IdNomeSetorEmail`)**: 
    *   *Por que?* Facilita a gestão de RH ao encontrar funcionários por qualquer dado parcial.

### 4. Módulo de Estoques/Depósitos (`/api/Estoques`)
*   **Saldo Global por Código de Barras**: 
    *   *Por que?* O usuário pode "bipar" um código de barras e o sistema varre todos os depósitos físicos da empresa para encontrar onde o item está localizado.

### 🛡️ Segurança e Melhores Práticas (Banco de Dados)

O sistema foi construído seguindo rigorosos padrões de segurança para garantir a integridade dos dados:

#### 1. Prevenção de SQL Injection
Todas as consultas SQL são realizadas utilizando **Parâmetros (@)** do Dapper. 
- **Por que?** Isso impede que entradas mal-intencionadas de usuários (como `' OR 1=1; --`) sejam interpretadas como comandos pelo SQL Server. O dado do usuário é tratado estritamente como um valor de busca, nunca como parte do comando executável.

#### 2. Buscas Inteligentes e Performance
Implementamos uma estratégia de busca simplificada que utiliza a concatenação diretamente no SQL:
```sql
WHERE (@Nome IS NULL OR Nome LIKE '%' + @Nome + '%')
```
- **Vantagem:** Essa técnica mantém o código C# limpo (sem necessidade de tratar porcentagens `%` no código) e permite que o SQL Server otimize o plano de execução, garantindo buscas rápidas mesmo com grandes volumes de dados.

#### 3. Tratamento de Dados Nulos e Vazios
No Backend, utilizamos `string.IsNullOrWhiteSpace` para converter strings vazias ou apenas espaços vindos do Frontend em `NULL`. Isso garante que o motor de busca do banco de dados ignore corretamente os filtros não preenchidos, retornando o resultado esperado pelo usuário de forma intuitiva.

#### 4. Segurança de Autenticação (BCrypt)
O sistema conta com o repositório `AuthRepository` preparado para integração com **BCrypt**, garantindo que as senhas dos funcionários sejam armazenadas apenas em formato de **Hash**, nunca em texto plano (disponível para ativação em produção).

---

### 🚀 Como Rodar o Projeto
1.  Clone o repositório.
2.  Configure a sua `DefaultConnection` no arquivo `appsettings.json`.
3.  Execute os scripts SQL fornecidos na pasta `/Database` (se houver).
4.  Abra o projeto no Visual Studio ou VS Code e execute `dotnet run`.

---

## 🏗️ Estrutura do Projeto
- **/Controllers**: Endpoints da API e tratamento de erros HTTP.
- **/Repository**: Lógica de acesso ao banco (Dapper) e cálculos de saldo (SUM/CASE).
- **/Models**: Definições de entidades e DTOs de pesquisa/solicitação.

---

## ⚠️ Observações Técnicas Importantes
- **Cálculo de Saldo**: O sistema utiliza *Conditional Aggregation* (SUM + CASE) no SQL.
  - IDs 1 e 4 = **Entradas** (+)
  - IDs 2 e 3 = **Saídas** (-)
- **Integridade Transacional**: Operações que envolvem Compra + Estoque rodam dentro de um `transaction.Commit()` para evitar inconsistência de dados.

---
*Documentação gerada para suporte ao desenvolvimento Frontend e Auditoria do Sistema.*
