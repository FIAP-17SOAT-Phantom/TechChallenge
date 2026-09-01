# Tech Challenge | Grupo Phantom

## Sistema Integrado de Atendimento e Execucao de Servicos

Backend de gestao de oficina mecanica desenvolvido com .NET 8, Clean Architecture, DDD, CQRS, MediatR, Entity Framework Core e PostgreSQL para o Tech Challenge da FIAP.

## Funcionalidades

- clientes e veiculos;
- catalogo de servicos;
- pecas, reservas, consumo e alertas de estoque;
- fluxo completo da Ordem de Servico;
- diagnostico e registro individual dos servicos executados;
- orcamento, revisao, aprovacao, rejeicao e nova versao;
- aprovacao e reserva de estoque em transacao serializable;
- indicadores administrativos;
- autenticacao JWT e autorizacao por roles;
- senha temporaria e troca obrigatoria no primeiro acesso;
- area autenticada do cliente;
- respostas de erro com `ProblemDetails`;
- Swagger/OpenAPI com autorizacao Bearer.

## Projetos

| Projeto | Responsabilidade |
|---------|------------------|
| `OficinaMecanica.API` | Controllers, autenticacao, autorizacao, erros e OpenAPI |
| `OficinaMecanica.Application` | Commands, Queries, Handlers, Validators e interfaces |
| `OficinaMecanica.Domain` | Aggregates, entidades, Value Objects, eventos e regras |
| `OficinaMecanica.Infrastructure` | PostgreSQL, EF Core, Identity, repositories e migrations |
| `OficinaMecanica.Tests` | Testes de Domain e Application |
| `OficinaMecanica.IntegrationTests` | Testes HTTP com WebApplicationFactory, Testcontainers e PostgreSQL real |

Direcao das dependencias:

```text
API -> Application -> Domain
Infrastructure -> Application / Domain
```

## Execucao com Docker

Pre-requisitos:

- Docker;
- Docker Compose.

Crie o arquivo `.env` a partir de `.env.example` e defina, no minimo:

```dotenv
JWT_SECRET=uma-chave-com-pelo-menos-32-bytes
ADMIN_PASSWORD=uma-senha-forte
ADMIN_EMAIL=admin@oficina.com
```

Suba API e PostgreSQL:

```bash
docker compose up --build
```

Enderecos:

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- OpenAPI JSON: `http://localhost:8080/swagger/v1/swagger.json`

As migrations sao aplicadas automaticamente na inicializacao. O administrador inicial e criado a partir das variaveis externas.

## Execucao local

O projeto carrega automaticamente o arquivo `.env` da raiz quando iniciado pelo Visual Studio ou por `dotnet run`. Variaveis de ambiente definidas no sistema possuem prioridade e nao sao sobrescritas.

Com PostgreSQL disponivel na connection string configurada:

```powershell
dotnet run --project src/OficinaMecanica.API/OficinaMecanica.API.csproj
```

Sem Docker, instale o PostgreSQL localmente e crie o banco `oficina_mecanica` com o usuario e senha definidos em `appsettings.json`, ou substitua `ConnectionStrings__DefaultConnection` no ambiente. As migrations sao aplicadas automaticamente.

## Primeiro acesso

1. Autentique o administrador em `POST /api/auth/login`.
2. Use `Authorize` no Swagger informando somente o token, sem escrever `Bearer`.
3. Cadastre Cliente, Veiculo, Servico e Peca.
4. Crie usuarios em `POST /api/auth/usuarios`.
5. Entregue a senha temporaria retornada apenas nessa resposta.
6. O usuario autentica e chama `POST /api/auth/alterar-senha`.
7. Depois da troca, realiza novo login para obter o token definitivo.

## Fluxo principal

```text
Criar Cliente e Veiculo
  -> Criar OS
  -> Iniciar e registrar diagnostico
  -> Gerar e revisar orcamento
  -> Enviar ao cliente
  -> Cliente aprovar
  -> Reservar estoque e iniciar execucao
  -> Registrar servicos executados
  -> Finalizar e consumir reservas
  -> Entregar veiculo
```

## Resumo da API

Todas as rotas, exceto o login, exigem JWT por padrao. As roles indicadas representam qualquer uma das roles autorizadas.

| Metodo | Endpoint | Roles | Finalidade |
|--------|----------|-------|------------|
| `POST` | `/api/auth/login` | Publico | Autenticar e emitir JWT |
| `POST` | `/api/auth/usuarios` | Admin | Criar usuario e senha temporaria |
| `POST` | `/api/auth/alterar-senha` | Autenticado | Trocar senha temporaria ou atual |
| `POST` | `/api/clientes` | Admin, Atendente | Cadastrar cliente |
| `POST` | `/api/veiculos` | Admin, Atendente | Vincular veiculo ao cliente |
| `POST` | `/api/servicos` | Admin | Cadastrar servico |
| `POST` | `/api/pecas` | Admin | Cadastrar peca e estoque inicial |
| `PATCH` | `/api/pecas/{pecaId}/estoque` | Admin | Registrar entrada de estoque |
| `POST` | `/api/ordens-de-servico` | Admin, Atendente | Abrir Ordem de Servico |
| `PATCH` | `/api/ordens-de-servico/{id}/registrar-diagnostico` | Admin, Mecanico | Registrar diagnostico e itens |
| `POST` | `/api/orcamentos` | Admin, Atendente | Gerar orcamento da OS |
| `PATCH` | `/api/orcamentos/{id}/enviar` | Admin, Atendente | Enviar orcamento ao cliente |
| `PATCH` | `/api/orcamentos/{id}/aprovar` | Cliente | Aprovar e reservar estoque |
| `PATCH` | `/api/orcamentos/{id}/rejeitar` | Cliente | Rejeitar orcamento |
| `PATCH` | `/api/ordens-de-servico/{id}/finalizar` | Admin, Mecanico | Finalizar e consumir reservas |
| `PATCH` | `/api/ordens-de-servico/{id}/entregar` | Admin, Atendente | Registrar entrega do veiculo |
| `GET` | `/api/ordens-de-servico/minhas` | Cliente | Listar as proprias Ordens de Servico |
| `GET` | `/api/ordens-de-servico/indicadores` | Admin | Consultar indicadores administrativos |

A referencia com exemplos completos de login, cadastros, Ordem de Servico, orcamento e respostas de erro esta em [Exemplos da API](docs/exemplos-api.md). O Swagger permanece como fonte completa e executavel das 42 rotas.

### Exemplo rapido de login

```http
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "email": "admin@oficina.local",
  "senha": "senha-configurada-no-env"
}
```

Resposta `200 OK`:

```json
{
  "token": "eyJ...",
  "expiraEm": "2026-09-01T20:00:00Z",
  "email": "admin@oficina.local",
  "roles": ["Admin"],
  "trocaSenhaObrigatoria": false
}
```

## Testes

A solucao possui duas suites:

| Suite | Projeto | Quantidade | Escopo |
|-------|---------|-----------:|--------|
| Unitarios | `OficinaMecanica.Tests` | 96 | Domain, Value Objects, aggregates e Application |
| Integracao | `OficinaMecanica.IntegrationTests` | 7 | HTTP, JWT, MediatR, Identity, EF Core e PostgreSQL 16 real |

Executar os testes unitarios:

```bash
dotnet test src/OficinaMecanica.Tests/OficinaMecanica.Tests.csproj
```

Executar os testes de integracao com Docker Desktop ativo:

```bash
dotnet test src/OficinaMecanica.IntegrationTests/OficinaMecanica.IntegrationTests.csproj
```

Executar toda a solucao:

```bash
dotnet test src/OficinaMecanica.slnx
```

Resultado validado: **103 testes aprovados e nenhuma falha**. A camada Domain possui **82,03% de cobertura de linhas** na medicao OpenCover atual; os percentuais por classe e as instrucoes para gerar o relatorio estao em [Testes e cobertura](docs/relatorio-testes-cobertura.md).

O pipeline do GitHub Actions executa build Release, testes unitarios com cobertura, testes de integracao com Testcontainers e scan de dependencias a cada push ou pull request para `main`.

## Seguranca e qualidade

- ASP.NET Core Identity armazena hashes de senha, nunca senhas em texto puro;
- JWT assinado com HMAC SHA-256, segredo externo e expiracao configuravel;
- autorizacao por roles `Admin`, `Atendente`, `Mecanico` e `Cliente`;
- politica global exige autenticacao, exceto em rotas explicitamente anonimas;
- usuario inativo e bloqueado mesmo que ainda possua token nao expirado;
- senha temporaria aleatoria e troca obrigatoria no primeiro acesso;
- validacao centralizada com FluentValidation;
- erros padronizados com `ProblemDetails`;
- scan NuGet direto e transitivo sem vulnerabilidades conhecidas na ultima validacao;
- cobertura de linhas de 82,03% na camada Domain.

Os detalhes e achados corrigidos estao no [Relatorio de vulnerabilidades](docs/relatorio-vulnerabilidades.md). A preparacao para analise de bugs, code smells, duplicacao, hotspots e Quality Gate esta descrita em [Qualidade com Sonar](docs/qualidade-sonar.md). Metricas Sonar somente devem ser publicadas depois de uma execucao real vinculada ao repositorio.

## Execucao local e deploy

O ambiente entregue e local: Docker Compose inicia API e PostgreSQL no computador do desenvolvedor. O projeto ainda nao declara uma URL publica nem um provedor de hospedagem.

Em um deploy, `JWT_SECRET`, credenciais administrativas e connection string devem ser fornecidos pelo gerenciador de secrets da plataforma. O banco deve usar volume persistente ou PostgreSQL gerenciado, com backup e acesso de rede restrito. As imagens e comandos atuais permitem evoluir para esse ambiente sem alterar as camadas internas da aplicacao.

## Documentacao

| Documento | Descricao |
|-----------|-----------|
| [Evolucao do backend](docs/evolucao-implementacao-backend.md) | Registro incremental de toda a implementacao |
| [Plano de execucao](docs/plano-execucao-tech-challenge-fase1.md) | Roadmap da fase |
| [Modelagem DDD e Event Storming](docs/plano-bloco1-modelagem-ddd-event-storming.md) | Bounded Contexts, eventos, comandos e aggregates |
| [Decisao arquitetural](docs/decisao-arquitetural-clean-architecture.md) | Clean Architecture |
| [Decisao de banco](docs/decisao-banco-de-dados-postgresql.md) | PostgreSQL |
| [Decisoes tecnicas](docs/decisoes-tecnicas-stack-completa.md) | Stack e justificativas |
| [Guia da API, Docker e PostgreSQL](docs/guia-api-docker-postgresql.md) | Funcionamento da API, containers, banco, volume, migrations e acesso aos dados |
| [Roadmap do login e JWT](docs/roadmap-login-autenticacao-jwt.md) | Caminho completo do Controller ao PostgreSQL, emissao do token e autorizacao |
| [Status de validacao da Fase 1](docs/status-validacao-fase1.md) | Auditoria de build, testes, Docker, seguranca e pendencias da entrega |
| [Exemplos da API](docs/exemplos-api.md) | Requisicoes e respostas dos principais fluxos |
| [Testes e cobertura](docs/relatorio-testes-cobertura.md) | Organizacao, execucao e cobertura dos dominios criticos |
| [Relatorio de vulnerabilidades](docs/relatorio-vulnerabilidades.md) | Scan de dependencias, achados e correcoes |
| [Qualidade com Sonar](docs/qualidade-sonar.md) | Metricas esperadas e configuracao pendente de credenciais externas |

## Grupo

Phantom | FIAP - Pos-graduacao em Software Architecture | Turma 17SOAT

| Membro | RM |
|--------|----|
| Gyovanna de Oliveira Carvalho | 376627 |
| Bruno Russo Ribeiro da Silva | 376557 |

Projeto academico - FIAP 2026.
