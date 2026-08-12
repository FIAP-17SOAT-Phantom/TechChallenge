# Decisao Arquitetural - Clean Architecture (Arquitetura Limpa)

## Contexto

Para o Tech Challenge Fase 1 (sistema de oficina mecanica com DDD), avaliamos quatro arquiteturas candidatas:

1. **Arquitetura em Camadas (Layered)**
2. **Arquitetura Hexagonal (Ports & Adapters)**
3. **Onion Architecture**
4. **Clean Architecture (Arquitetura Limpa)**

## Comparativo

| Arquitetura | Essencia | Pros | Contras |
|-------------|----------|------|---------|
| Camadas | Dependencia linear top-down | Simples, muito material .NET | Dominio acoplado a infra |
| Hexagonal | Dominio no centro com ports/adapters | Excelente isolamento | Nomenclatura confunde iniciantes |
| Onion | Camadas concentricas | Dependencias claras pra dentro | Menos convencoes em .NET |
| Clean Architecture | Dependencia invertida com convencoes claras | Vasta doc em .NET, combina com DDD | Mais boilerplate |

## Decisao: Clean Architecture

Escolhemos **Clean Architecture** pelos seguintes motivos:

### 1. Combina naturalmente com DDD

A Clean Architecture ja organiza o projeto exatamente como modelamos no Event Storming:

- **Domain Layer** = Entities, Aggregates, Value Objects, Domain Events, Enums
- **Application Layer** = Use Cases (comandos/queries), interfaces de repositorio, event handlers
- **Infrastructure Layer** = EF Core, repositorios concretos, servicos externos
- **API Layer** = Controllers, DTOs

Isso bate 1:1 com a estrutura de pastas definida no plano de modelagem.

### 2. Grupo iniciante - muita referencia em .NET

Existe abundancia de templates, cursos e repositorios de referencia em C# usando Clean Architecture:

- Template do Jason Taylor (CleanArchitecture no GitHub)
- Ardalis CleanArchitecture template
- Conteudo da propria Microsoft

Hexagonal e Onion tem menos material pratico em .NET, o que aumentaria a curva de aprendizado.

### 3. Regra de dependencia clara e visual

A regra e simples: **dependencias sempre apontam pra dentro**.

- Infrastructure depende de Application (implementa interfaces)
- Application depende de Domain (usa entities e regras)
- Domain nao depende de nada externo

Facil de explicar, fiscalizar em code review e demonstrar para a banca.

### 4. Testabilidade

Os Use Cases dependem de interfaces (definidas na Application Layer). Nos testes, mockamos repositorios e servicos externos sem tocar em banco. Essencial para:

- Atingir a cobertura de 80% exigida nos dominios criticos
- Demonstrar qualidade no video de entrega
- Rodar testes rapidamente no CI/CD

### 5. Bounded Contexts se encaixam bem

Cada BC (Atendimento, Oficina, Orcamentacao, Estoque, Catalogo) vira uma pasta/namespace dentro de Domain e Application, mantendo a separacao logica sem precisar de microsservicos - que seriam overengineering para este projeto.

### 6. Suporte a CQRS simplificado

Com Clean Architecture + MediatR, implementamos facilmente o padrao Command/Query:

- **Commands** = operacoes que alteram estado (CriarOS, AprovarOrcamento, ReservarPeca)
- **Queries** = consultas que apenas leem dados (ConsultarStatusOS, ListarPecas)

Alinha diretamente com os Comandos identificados no Event Storming.

## Estrutura resultante

 src/
 OficinaMecanica.Domain/ <- Nucleo (zero dependencias externas)
 Atendimento/
 Oficina/
 Orcamentacao/
 Estoque/
 CatalogoServicos/
 OficinaMecanica.Application/ <- Use Cases + Interfaces
 Common/ (Behaviors, Exceptions, Interfaces)
 UseCases/ (Atendimento, Oficina, Orcamentacao, Estoque)
 EventHandlers/
 OficinaMecanica.Infrastructure/ <- Implementacoes concretas
 Persistence/ (DbContext, Repositories, Migrations)
 Services/
 DependencyInjection.cs
 OficinaMecanica.API/ <- Presentation / Entry point
 Controllers/
 DTOs/
 Middleware/
 Program.cs
 OficinaMecanica.Tests/
 Unit/
 Integration/

## Diagrama de dependencias

 API --> Application --> Domain <-- Infrastructure

> Nota: Infrastructure depende de Application (para implementar as interfaces) e de Domain (para mapear entities). Nunca o contrario.

## Tecnologias complementares

| Camada | Tecnologia | Proposito |
|--------|-----------|-----------|
| Domain | Puro C# | Sem dependencias externas |
| Application | MediatR | CQRS (Commands/Queries) |
| Application | FluentValidation | Validacao de inputs nos Use Cases |
| Infrastructure | Entity Framework Core | ORM / Persistencia |
| Infrastructure | PostgreSQL | Banco de dados relacional |
| API | ASP.NET Core 8 | Web framework |
| API | JWT Bearer | Autenticacao |
| API | Swagger/OpenAPI | Documentacao de endpoints |
| Tests | xUnit + Moq | Testes unitarios e mocks |

## Conclusao

Clean Architecture e essencialmente a mesma filosofia da Hexagonal e da Onion (inversao de dependencia, dominio isolado), mas com nomenclatura e convencoes que o ecossistema .NET ja adotou amplamente. Para o nosso grupo - iniciante em DDD - isso significa:

- Mais material de apoio disponivel
- Menos confusao com terminologia
- Estrutura alinhada com o plano de modelagem
- Facilidade de demonstrar para a banca que entendemos a arquitetura

**Status:** Aprovada pela equipe
