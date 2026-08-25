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
2. Use `Authorize` no Swagger com `Bearer {token}`.
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

## Testes

```bash
dotnet test src/OficinaMecanica.Tests/OficinaMecanica.Tests.csproj
```

Os testes de integracao com PostgreSQL real permanecem separados da finalizacao do codigo e dependem de Docker/Testcontainers.

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

## Grupo

Phantom | FIAP - Pos-graduacao em Software Architecture | Turma 17SOAT

| Membro | RM |
|--------|----|
| Gyovanna de Oliveira Carvalho | 376627 |
| Bruno Russo Ribeiro da Silva | 376557 |

Projeto academico - FIAP 2026.
