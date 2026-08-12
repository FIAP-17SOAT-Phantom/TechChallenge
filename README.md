# Tech Challenge - Fase 1 | Grupo Phantom

## Sistema Integrado de Atendimento e Execucao de Servicos (Oficina Mecanica)

Sistema de gestao de uma oficina mecanica desenvolvido com **Domain-Driven Design (DDD)**, **Clean Architecture** e **.NET 8**, como parte do Tech Challenge da pos-graduacao em Software Architecture (FIAP - Turma 17SOAT).

---

## Funcionalidades

- Cadastro de clientes e veiculos
- Criacao e acompanhamento de Ordens de Servico (OS)
- Geracao de orcamento com aprovacao/rejeicao/renegociacao
- Controle de estoque de pecas e insumos com reserva automatica
- Maquina de estados da OS (Recebida > Em Diagnostico > Aguardando Aprovacao > Em Execucao > Finalizada > Entregue)
- Autenticacao JWT com roles (Admin, Atendente, Mecanico, Cliente)
- Documentacao via Swagger/OpenAPI

---

## Stack Tecnica

| Camada | Tecnologia |
|--------|-----------|
| Runtime | .NET 8 (LTS) |
| Web Framework | ASP.NET Core 8 |
| ORM | Entity Framework Core 8 |
| Banco de Dados | PostgreSQL 16 |
| CQRS | MediatR 12 |
| Validacao | FluentValidation 11 |
| Autenticacao | JWT Bearer |
| Documentacao API | Swagger (Swashbuckle) |
| Containers | Docker + Docker Compose |
| Testes | xUnit + Moq + TestContainers |

---

## Arquitetura

**Clean Architecture** com inversao de dependencia:

``r
API --> Application --> Domain <-- Infrastructure
``r

| Projeto | Responsabilidade |
|---------|-----------------|
| OficinaMecanica.API | Controllers, DTOs, Middleware JWT, Swagger |
| OficinaMecanica.Application | Use Cases, Commands/Queries, Validators, Interfaces |
| OficinaMecanica.Domain | Entities, Aggregates, Value Objects, Domain Events, Enums |
| OficinaMecanica.Infrastructure | EF Core, Repositories, DbContext, Migrations |
| OficinaMecanica.Tests | Testes unitarios e de integracao |

---

## Como executar

### Pre-requisitos

- Docker e Docker Compose instalados
- (Opcional) .NET 8 SDK para desenvolvimento local

### Subir o ambiente completo

`ash
docker compose up --build
``r

A API estara disponivel em: http://localhost:8080
Swagger: http://localhost:8080/swagger

### Rodar localmente (sem Docker)

`ash
# Subir apenas o banco
docker compose up db -d

# Rodar a API
cd src/OficinaMecanica.API
dotnet run
``r

### Rodar testes

`ash
cd src
dotnet test
``r

---

## Bounded Contexts (DDD)

| # | Bounded Context | Responsabilidade |
|---|----------------|-----------------|
| 1 | Atendimento | Cadastro de clientes e veiculos, recepcao |
| 2 | Oficina | Ordem de Servico, diagnostico, execucao |
| 3 | Orcamentacao | Orcamento, aprovacao/rejeicao/renegociacao |
| 4 | Estoque | Pecas, controle de quantidade, reserva |
| 5 | Catalogo de Servicos | Tipos de servico, preco base |

---

## Documentacao

| Documento | Descricao |
|-----------|-----------|
| [Plano de Execucao](docs/plano-execucao-tech-challenge-fase1.md) | Roadmap completo dos 5 blocos |
| [Modelagem DDD + Event Storming](docs/plano-bloco1-modelagem-ddd-event-storming.md) | Bounded Contexts, eventos, comandos, agregados |
| [Decisao Arquitetural](docs/decisao-arquitetural-clean-architecture.md) | Por que Clean Architecture |
| [Decisao Banco de Dados](docs/decisao-banco-de-dados-postgresql.md) | Por que PostgreSQL |
| [Decisoes Tecnicas](docs/decisoes-tecnicas-stack-completa.md) | Stack completa e justificativas |

---

## Event Storming (Miro)

Board: https://miro.com/app/board/uXjVHy2wodY=/

Conteudo do board:
- Context Map com 5 Bounded Contexts
- Timeline do fluxo da Ordem de Servico (15 eventos)
- Timeline do fluxo de Gestao de Estoque (6 eventos)
- Linguagem Ubiqua (glossario com 14 termos)
- Maquina de Estados da OS
- Diagrama de Arquitetura em Camadas

---

## Estrutura do Repositorio

``r
Tech_Challenge_Fase1_FIAP/
|-- .gitignore
|-- docker-compose.yml
|-- README.md
|-- docs/
| |-- decisao-arquitetural-clean-architecture.md
| |-- decisao-banco-de-dados-postgresql.md
| |-- decisoes-tecnicas-stack-completa.md
| |-- plano-bloco1-modelagem-ddd-event-storming.md
| |-- plano-execucao-tech-challenge-fase1.md
| |-- miro-import/ (CSVs para importacao no Miro)
|-- src/
 |-- OficinaMecanica.slnx
 |-- OficinaMecanica.API/
 |-- OficinaMecanica.Application/
 |-- OficinaMecanica.Domain/
 |-- OficinaMecanica.Infrastructure/
 |-- OficinaMecanica.Tests/
``r

---

## Grupo

**Phantom** | FIAP - Pos-graduacao em Software Architecture | Turma 17SOAT

| Membro | Discord |
|--------|---------|
| (nome) | (user) |
| (nome) | (user) |
| (nome) | (user) |
| (nome) | (user) |

---

## Licenca

Projeto academico - FIAP 2026
