# CP5 - Advanced Business Development with .NET — API de Controle de Estoque

API RESTful em **.NET 8 (Controllers)** para controle de estoque de **fornecedores** e **produtos**, construída com **Clean Architecture** e aplicando Operation Result, erros customizados (retornados e lançados pelo Repository), integração resiliente com o serviço externo **ViaCEP**, paginação com **PageResult**, **HATEOAS** e autenticação/autorização com **JWT** por perfis.

## 👤 Integrante

| Nome | RM | Turma |
|---|---|---|
| Gabriel Cabral Mendes Mariano | 563230 | 2TDSPO |

## 📹 Vídeo de demonstração

**Link:** _adicionar aqui o link do vídeo (YouTube não-listado / Google Drive público)_

---

## 🧭 Descrição do domínio

- **Fornecedor**: razão social, CNPJ (único), e-mail, telefone e endereço. O endereço é preenchido automaticamente a partir do CEP consultado no **ViaCEP**.
- **Produto**: nome, descrição, SKU (único), preço, quantidade em estoque e fornecedor. Possui regra de negócio de ajuste de estoque (não permite saldo negativo).
- **Usuário**: usado para autenticação JWT, com perfis `Admin` (leitura e escrita) e `Leitor` (somente leitura).

## 🏛️ Arquitetura

```
Estoque.sln
├── src
│   ├── Estoque.Domain          → Entidades, Value Objects, OperationResult, PageResult, Errors, Exceptions, interfaces de repositório
│   ├── Estoque.Application     → DTOs, interfaces de serviços, serviços de aplicação (casos de uso), mapeamentos
│   ├── Estoque.Infrastructure  → EF Core (DbContext, mapeamentos), repositórios, ViaCEP (HttpClient + resiliência), JWT, hash de senha
│   └── Estoque.API             → Controllers, HATEOAS, tratamento global de exceções, autenticação/autorização, Swagger
└── tests
    ├── Estoque.UnitTests         → Testes de unidade (domínio, serviços, ViaCEP com HttpMessageHandler fake, segurança)
    └── Estoque.IntegrationTests  → Testes funcionais com WebApplicationFactory no MESMO banco da aplicação
```

Dependências entre camadas: `API → Application/Infrastructure → Domain`. O Domain não depende de nenhuma outra camada.

## ✅ Onde cada requisito foi implementado

| Critério | Implementação |
|---|---|
| **Clean Architecture** | 4 projetos separados (`Domain`, `Application`, `Infrastructure`, `API`) com injeção de dependência por camada (`AddApplication`, `AddInfrastructure`). |
| **Otimização** | `AsNoTracking` em todas as leituras, paginação executada no banco (`Skip/Take` + `CountAsync`), índices (`CNPJ` e `SKU` únicos, `Nome`, `FornecedorId`, `Uf`), `ExecuteDeleteAsync` na remoção de produto (1 ida ao banco), cache em memória das consultas de CEP (6h), compressão de resposta Brotli/Gzip e omissão de campos nulos no JSON. |
| **Testes automatizados** | `Estoque.UnitTests` (unidade) e `Estoque.IntegrationTests` (funcionais, via `WebApplicationFactory<Program>`, usando o banco configurado na aplicação, sem SQLite/InMemory). |
| **Operation Result** | `Domain/Common/OperationResult.cs`: `OperationResult` e `OperationResult<T>` com `IsSuccess`, `Data`, `Error`, `Map` e conversão implícita de `Error`. Todos os serviços retornam `OperationResult` e os controllers convertem para HTTP. |
| **Erros customizados** | `Domain/Errors/DomainErrors.cs` (catálogo de erros: `FornecedorErrors`, `ProdutoErrors`, `CepErrors`, `AuthErrors`) e `Domain/Exceptions/DomainExceptions.cs` (`EntityNotFoundException`, `DuplicateEntityException`, `BusinessRuleException`, `DomainValidationException`, `RepositoryException`). |
| ↳ **Retorno no Repository** | `FornecedorRepository` retorna `OperationResult` com o erro (`GetByIdAsync`, `UpdateAsync`, `DeleteAsync` → `Fornecedor.NaoEncontrado`, `Fornecedor.PossuiProdutos`). |
| ↳ **Lançamento de erro no Repository** | `ProdutoRepository` lança exceções customizadas (`EntityNotFoundException`, `DuplicateEntityException`, `RepositoryException`), capturadas no serviço e convertidas em `OperationResult`. O `GlobalExceptionHandler` (API) garante resposta padronizada para qualquer exceção não tratada. |
| **Serviço externo** | `Infrastructure/ExternalServices/ViaCepService.cs`: `HttpClient` tipado com `AddStandardResilienceHandler` (retry, timeout por tentativa, timeout total e circuit breaker), tratamento de CEP inválido, CEP inexistente, falhas HTTP e de rede, e cache. |
| **PageResult** | `Domain/Common/PageResult.cs` com `Items`, `PageNumber`, `PageSize`, `TotalItems`, `TotalPages`, `HasPrevious`, `HasNext`; `PageRequest` normaliza página e limita o tamanho máximo a 50. |
| **HATEOAS** | `API/Hateoas` (`Link`, `Resource<T>`, `PagedResource<T>`). Cada item tem links `self`, `produtos`/`fornecedor` e, apenas para `Admin`, `update`, `delete` e `ajustar-estoque`. Listagens trazem `self`, `first`, `previous`, `next`, `last`. |
| **Autenticação/Autorização** | JWT Bearer (`/api/v1/auth/login`), senhas com hash PBKDF2, políticas `Leitura` (Admin e Leitor) e `SomenteAdmin` (escrita). Respostas 401/403 padronizadas com código de erro. |

## 🔌 Endpoints

| Método | Rota | Perfil |
|---|---|---|
| POST | `/api/v1/auth/login` | Anônimo |
| GET | `/api/v1/auth/me` | Autenticado |
| GET | `/api/v1/fornecedores?pageNumber=1&pageSize=10&uf=SP` | Admin, Leitor |
| GET | `/api/v1/fornecedores/{id}` | Admin, Leitor |
| POST | `/api/v1/fornecedores` | Admin |
| PUT | `/api/v1/fornecedores/{id}` | Admin |
| DELETE | `/api/v1/fornecedores/{id}` | Admin |
| GET | `/api/v1/produtos?pageNumber=1&pageSize=10&fornecedorId=1&nome=mouse` | Admin, Leitor |
| GET | `/api/v1/produtos/{id}` | Admin, Leitor |
| POST | `/api/v1/produtos` | Admin |
| PUT | `/api/v1/produtos/{id}` | Admin |
| PATCH | `/api/v1/produtos/{id}/estoque` | Admin |
| DELETE | `/api/v1/produtos/{id}` | Admin |
| GET | `/api/v1/cep/{cep}` | Admin, Leitor |
| GET | `/health` | Anônimo |

### Usuários criados automaticamente

| Usuário | Senha | Perfil |
|---|---|---|
| `admin` | `Admin@123` | Admin |
| `leitor` | `Leitor@123` | Leitor |

### Exemplo de erro padronizado

```json
{
  "type": "https://httpstatuses.io/404",
  "title": "Recurso não encontrado",
  "status": 404,
  "detail": "Produto com id 99 não foi encontrado.",
  "instance": "/api/v1/produtos/99",
  "success": false,
  "code": "Produto.NaoEncontrado",
  "errorType": "NotFound",
  "traceId": "0HN..."
}
```

| Tipo de erro | Status HTTP |
|---|---|
| Validation | 400 |
| Unauthorized | 401 |
| Forbidden | 403 |
| NotFound | 404 |
| Conflict | 409 |
| ExternalService | 502 |
| Unexpected | 500 |

## ⚙️ Configurações necessárias

### Pré-requisitos

- .NET SDK 8 (ou Visual Studio 2022 com a carga de trabalho ASP.NET)
- SQL Server LocalDB (instalado junto com o Visual Studio) **ou** Oracle
- Internet para o ViaCEP (`https://viacep.com.br/ws/`)

### Banco de dados

O banco padrão é o **SQL Server LocalDB**, já configurado no `appsettings.json`:

```json
"Database": { "Provider": "SqlServer" },
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=Cp5Estoque;Trusted_Connection=True;TrustServerCertificate=True"
}
```

Não é necessário criar nada: o banco `Cp5Estoque`, as tabelas (`CP5_FORNECEDORES`, `CP5_PRODUTOS`, `CP5_USUARIOS`) e os dados iniciais (3 fornecedores, 12 produtos e 2 usuários) são criados automaticamente na primeira execução. Para conferir se o LocalDB está instalado, rode `sqllocaldb info` (deve listar `MSSQLLocalDB`).

O projeto também suporta **Oracle**. Basta alterar `Database:Provider` para `Oracle` e informar a connection string, de preferência via user-secrets para não expor a senha:

```bash
dotnet user-secrets set "Database:Provider" "Oracle" --project src/Estoque.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/ORCL" --project src/Estoque.API
```

A API e os testes de integração usam **sempre a mesma configuração**, portanto os testes rodam no mesmo banco da aplicação.

### JWT

Configurado em `appsettings.json` na seção `Jwt` (`Issuer`, `Audience`, `SecretKey` com no mínimo 32 caracteres e `ExpirationMinutes`).

### Serviço externo

`ExternalServices:ViaCep:BaseUrl` (padrão `https://viacep.com.br/ws/`).

## ▶️ Como rodar

```bash
dotnet restore
dotnet build
dotnet run --project src/Estoque.API
```

Acesse o Swagger em **http://localhost:5080/swagger**.

1. Execute `POST /api/v1/auth/login` com `admin` / `Admin@123`.
2. Copie o `accessToken`, clique em **Authorize** e cole o token.
3. Teste os endpoints.

## 🧪 Como executar os testes

```bash
dotnet test
```

- **Testes de unidade**: não dependem de banco nem de rede.
- **Testes funcionais/integração**: sobem a API com `WebApplicationFactory` e usam o **mesmo banco configurado na aplicação** (mesma connection string). Somente o ViaCEP é substituído por um serviço de teste para que a suíte não dependa da internet. Os registros criados pelos testes usam CNPJ/SKU aleatórios e são removidos ao final.
