# Decisoes Tecnicas - Stack Completa

## Resumo das Decisoes

| Ponto | Decisao | Lib/Tool |
|-------|---------|----------|
| .NET | 8 (LTS) | - |
| Auth | JWT + Roles | ASP.NET Core Identity simplificado |
| CQRS | Sim | MediatR |
| Validacao | Application Layer | FluentValidation |
| Mapeamento | Manual | Extension methods |
| Erros | Result Pattern + ProblemDetails | Classe Result customizada |
| Docker | Compose puro | yml |
| Git | Trunk-based + feature branches | - |
| Testes | Unit + Integration | xUnit + Moq + TestContainers |
| CI/CD | GitHub Actions | Workflow basico |

---

## 1. Versao do .NET: .NET 8

**Decisao:** .NET 8 (LTS)

**Justificativa:**
- Versao LTS com suporte ate novembro de 2026
- 99 por cento dos tutoriais e templates de Clean Architecture sao pra .NET 8
- Performance excelente, Minimal APIs maduras, EF Core 8 com muitas melhorias
- .NET 9 existe mas nao e LTS - pode ter breaking changes menores
- A banca nao vai questionar a escolha de um LTS

---

## 2. Autenticacao/Autorizacao: JWT com Roles

**Decisao:** JWT Bearer Token com autorizacao baseada em Roles

### Roles definidas:

| Role | Quem e | O que pode fazer |
|------|--------|-----------------|
| Admin | Gerente da oficina | Tudo (CRUD de tudo, relatorios) |
| Atendente | Recepcionista | Cadastrar clientes/veiculos, criar OS, enviar orcamento |
| Mecanico | Tecnico | Registrar diagnostico, executar servicos, dar baixa em pecas |
| Cliente | Dono do veiculo | Consultar status da OS, aprovar/rejeitar orcamento |

### Justificativa:
- Roles e mais simples de implementar e demonstrar que Claims
- Claims faz sentido pra permissoes granulares - overengineering pro escopo
- Authorize(Roles = Admin,Atendente) e direto e legivel
- A banca entende Roles facilmente

### Fluxo:
- Login retorna JWT com role embutido no token
- Endpoints protegidos verificam role
- Cliente faz login proprio (email + senha) e so ve suas OS

---

## 3. CQRS: MediatR

**Decisao:** Usar MediatR para separacao Command/Query

### Justificativa:
- Se encaixa perfeitamente com Clean Architecture - cada Use Case vira um IRequest/IRequestHandler
- Pipeline Behaviors permitem validacao automatica, logging e transaction handling sem repetir codigo
- E o padrao de mercado em .NET quando se menciona CQRS
- Separa claramente Commands (escrita) de Queries (leitura)
- Template do Jason Taylor usa MediatR - podemos nos basear nele

### Exemplo:

 // Command
 public record CriarOrdemDeServicoCommand(Guid ClienteId, Guid VeiculoId) : IRequest<Guid>;

 // Handler
 public class CriarOrdemDeServicoHandler : IRequestHandler<CriarOrdemDeServicoCommand, Guid>
 {
 public async Task<Guid> Handle(CriarOrdemDeServicoCommand request, CancellationToken ct)
 {
 // logica do use case
 }
 }

### Pacote NuGet:
- MediatR (versao 12+)

---

## 4. Validacao: FluentValidation

**Decisao:** FluentValidation na Application Layer integrado com MediatR via Pipeline Behavior

### Justificativa:
- Validacao fica na Application Layer (nao polui o Domain com regras de input)
- Integra com MediatR - toda request e validada automaticamente antes de chegar no handler
- Sintaxe expressiva e facil de ler
- Facil de testar (cada validator e uma classe unitaria)
- Evita Data Annotations que acoplam validacao ao modelo

### Exemplo:

 public class CriarClienteValidator : AbstractValidator<CriarClienteCommand>
 {
 public CriarClienteValidator()
 {
 RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
 RuleFor(x => x.Cpf).Must(ValidarCpf).WithMessage("CPF invalido");
 RuleFor(x => x.Email).EmailAddress();
 }
 }

### Pacote NuGet:
- FluentValidation.DependencyInjectionExtensions

---

## 5. Mapeamento: Manual (Extension Methods)

**Decisao:** Mapeamento manual usando extension methods

### Justificativa:
- O projeto tem ~5 agregados com DTOs relativamente simples
- AutoMapper esconde bugs (esqueceu de mapear um campo = null silencioso)
- Mapeamento manual e explicito, refatoravel, e o compilador pega erros
- Pra 10-15 mapeamentos, nao justifica uma lib extra
- A banca ve exatamente o que esta acontecendo

### Exemplo:

 public static class OrdemDeServicoMappings
 {
 public static OrdemDeServicoDto ToDto(this OrdemDeServico os)
 {
 return new OrdemDeServicoDto
 {
 Id = os.Id,
 Numero = os.Numero,
 Status = os.Status.ToString()
 };
 }
 }

### Alternativa aceita:
Se o grupo preferir menos codigo repetitivo, Mapster e uma boa alternativa (mais rapido que AutoMapper, menos magico).

---

## 6. Tratamento de Erros: Result Pattern + ProblemDetails

**Decisao:** Result Pattern para logica de negocio + ProblemDetails para respostas HTTP

### Por que Result Pattern:
- Exceptions para fluxo de negocio e anti-pattern (excecao = algo inesperado, nao orcamento ja aprovado)
- Result pattern torna explicito o que pode dar errado
- Facilita testes - assert no resultado, nao precisa catch
- Implementacao: classe Result<T> simples customizada (nao precisa lib externa)

### Exemplo:

 public Result<Guid> AprovarOrcamento(Guid orcamentoId)
 {
 if (orcamento.Status != StatusOrcamento.Enviado)
 return Result.Failure<Guid>("Orcamento nao esta em status valido");
 return Result.Success(orcamento.Id);
 }

### Por que ProblemDetails:
- Padrao RFC 7807 para respostas de erro em APIs HTTP
- ASP.NET Core 8 tem suporte nativo
- Response padronizado e profissional
- A banca ve que seguimos padroes HTTP

### Exemplo de resposta:

 {
 "type": "https://tools.ietf.org/html/rfc7807",
 "title": "Validation Error",
 "status": 400,
 "detail": "CPF informado e invalido",
 "instance": "/api/clientes"
 }

---

## 7. Docker: Compose Puro

**Decisao:** Arquivo yml de compose sem .NET Aspire

### Justificativa:
- O enunciado pede que o ambiente suba com um unico comando
- Compose puro e o mais direto e facil de demonstrar
- .NET Aspire adiciona complexidade desnecessaria pro escopo
- Menos coisas pra explicar no video

### Servicos definidos:
- **api**: build da aplicacao ASP.NET Core na porta 8080
- **db**: PostgreSQL 16 alpine na porta 5432 com healthcheck
- Volume persistente para dados do banco
- depends_on com condition service_healthy

---

## 8. Git Strategy: Trunk-based Simplificado

**Decisao:** Branch main estavel + feature branches curtas

### Justificativa:
- GitFlow e pesado demais pra 4-5 pessoas em projeto de 4-8 semanas
- Trunk-based com feature branches curtas reduz conflitos de merge
- Cada membro trabalha no seu modulo - pouco conflito natural

### Regras:
- Branch main = sempre estavel
- Feature branches curtas: feature/crud-clientes, feature/fluxo-os
- PRs pequenos com review de pelo menos 1 outro membro
- Sem branches de release (projeto academico)

### Divisao sugerida entre membros:

| Membro | Responsabilidade |
|--------|-----------------|
| Pessoa 1 | Modulo Atendimento (Cliente + Veiculo) + Setup inicial |
| Pessoa 2 | Modulo Oficina (OS + maquina de estados) |
| Pessoa 3 | Modulo Orcamentacao + Estoque (reserva/baixa) |
| Pessoa 4 | Seguranca (JWT) + Testes + Docker + CI |

---

## 9. Testes: xUnit + Moq + TestContainers

**Decisao:** xUnit para framework, Moq para mocking, TestContainers para integracao

### Estrategia:

| Tipo | Ferramenta | O que testar |
|------|-----------|--------------|
| Unitario | xUnit + Moq | Regras de dominio (transicoes de status, validacao CPF, calculo orcamento) |
| Integracao | xUnit + TestContainers | Endpoints API completos com PostgreSQL real |
| Cobertura | coverlet | Medir percentual e gerar relatorio |

### Justificativas:
- **xUnit**: padrao da comunidade .NET, usado pela propria Microsoft
- **Moq**: simples, expressivo, o mais popular pra mocking em .NET
- **TestContainers**: sobe PostgreSQL real no Docker durante o teste - garante que o que funciona no teste funciona em producao
- **Por que nao InMemory**: banco in-memory se comporta diferente do PostgreSQL real (transacoes, constraints)

### Meta de cobertura:
- 80 por cento nos dominios criticos (OrdemDeServico, Orcamento, Estoque)
- Nao precisa 80 por cento em Controllers/DTOs

### Pacotes NuGet:
- xunit
- xunit.runner.visualstudio
- Moq
- Testcontainers.PostgreSql
- Microsoft.AspNetCore.Mvc.Testing
- coverlet.collector

---

## 10. CI/CD: GitHub Actions

**Decisao:** Pipeline basico de build + test no GitHub Actions

### Justificativa:
- A banca ve que testes rodam automaticamente em todo push
- Protege contra alguem quebrar o build sem perceber
- Gratuito pra repositorios privados (2000 min/mes no GitHub)
- Valoriza o projeto na avaliacao sem esforco grande

### Workflow basico:
- Trigger: push e pull_request
- Runner: ubuntu-latest
- Service: PostgreSQL 16 alpine
- Steps: checkout, setup-dotnet 8.0.x, restore, build, test com cobertura

---

## Stack Completa Consolidada

| Camada | Tecnologia | Versao |
|--------|-----------|--------|
| Runtime | .NET | 8 LTS |
| Web Framework | ASP.NET Core | 8 |
| ORM | Entity Framework Core | 8 |
| Banco | PostgreSQL | 16 |
| CQRS | MediatR | 12+ |
| Validacao | FluentValidation | 11+ |
| Auth | JWT Bearer | Nativo ASP.NET Core |
| Docs API | Swagger/OpenAPI | Swashbuckle |
| Containers | Docker + Compose | - |
| Testes | xUnit + Moq + TestContainers | - |
| Cobertura | coverlet | - |
| CI/CD | GitHub Actions | - |
| Versionamento | Git + GitHub | - |

**Status:** Aprovada pela equipe
