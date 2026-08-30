# Relatório de Testes e Cobertura

## Resumo

| Métrica | Valor |
|---------|-------|
| Total de testes | **96** |
| Testes passando | **96 (100%)** |
| Framework | xUnit 2.9.2 |
| Mocking | Moq 4.20.72 |
| Cobertura (ferramenta) | coverlet + ReportGenerator |

## Testes por área

| Área | Arquivos de teste | Foco |
|------|-------------------|------|
| Atendimento | CpfTests, EmailTests, PlacaTests, ClienteTests, VeiculoTests | Value Objects com validação + aggregates |
| Oficina | OrdemDeServicoTests | Máquina de estados da OS |
| Orcamentacao | OrcamentoTests, OrcamentoItensTests | Transições + gestão de itens |
| Estoque | PecaTests, AlertaEstoqueTests | Reserva/consumo + alertas |
| CatalogoServicos | ServicoTests | CRUD de serviços |
| Application | AprovarOrcamentoHandlerTests | Use case + política de reserva |

## Cobertura dos domínios críticos

Última medição da camada Domain (linhas):

| Classe (Aggregate/VO) | Cobertura |
|----------------------|-----------|
| Cliente | 96% |
| Veiculo | 91% |
| Orcamento | 92% |
| Servico | 91% |
| Peca | 86% |
| Reserva | 93% |
| OrdemDeServico | 84% |
| ItemOrcamento | 91% |
| Cpf | 97% |
| Email | 92% |
| Placa | 91% |
| AlertaEstoque | coberto (8 testes adicionados) |

Os domínios críticos (regras de negócio: OS, Orçamento, Estoque, Cliente) estão **acima de 80%**, cumprindo o requisito.

## Observação sobre medição local (Windows)

A máquina de desenvolvimento tem o **Windows Smart App Control** ativo (`VerifiedAndReputablePolicyState = 1`). Esse recurso de segurança bloqueia os assemblies instrumentados pelo coverlet quando executados a partir do diretório do projeto:

```
System.IO.FileLoadException: Could not load file or assembly 'OficinaMecanica.Domain.dll'.
Uma política de Controle de Aplicativo bloqueou este arquivo. (0x800711C7)
```

**Importante:** isso NÃO é um problema do código nem dos testes. Sem instrumentação, os 96 testes passam normalmente. A instrumentação de cobertura reescreve o DLL, e o Smart App Control bloqueia o arquivo modificado por não ter assinatura confiável.

### Como medir a cobertura de forma confiável

1. **Via CI (recomendado):** o pipeline `.github/workflows/ci.yml` roda em Ubuntu, sem Smart App Control. Gera relatório HTML + resumo automático a cada push.

2. **Localmente (contornando o WDAC):** copiar os binários para fora do diretório do projeto:
   ```powershell
   $tmp = "$env:TEMP\cov_run"
   Copy-Item "src\OficinaMecanica.Tests\bin\Debug\net8.0\*" $tmp -Recurse
   dotnet test "$tmp\OficinaMecanica.Tests.dll" --collect:"XPlat Code Coverage"
   ```

3. **Desativar temporariamente o Smart App Control** (Configurações do Windows → Privacidade e segurança → Smart App Control). Requer reinstalação do Windows para reativar, então não recomendado.

## Testes de Integração

Projeto: `OficinaMecanica.IntegrationTests`

Usa **TestContainers** para subir um PostgreSQL 16 real em Docker e **WebApplicationFactory** para hospedar a API em memória. Exercitam o pipeline completo (HTTP → Controller → MediatR → Identity/EF Core → banco real).

| Cenário | Teste |
|---------|-------|
| Login com credenciais válidas retorna token | AutenticacaoIntegrationTests |
| Login com senha incorreta é rejeitado | AutenticacaoIntegrationTests |
| Login com email inválido retorna 400 | AutenticacaoIntegrationTests |
| Rota protegida sem token retorna 401 | AutorizacaoIntegrationTests |
| Rota protegida com token inválido retorna 401 | AutorizacaoIntegrationTests |
| Criar + consultar serviço persiste no banco | ServicosFluxoIntegrationTests |
| Criar serviço com dados inválidos retorna 400 | ServicosFluxoIntegrationTests |

**Requisito:** Docker em execução. Os testes rodam automaticamente no CI (runner Ubuntu já tem Docker). Localmente, requerem Docker Desktop instalado.

## Comando padrão de teste

```bash
# Testes unitários
dotnet test src/OficinaMecanica.Tests/OficinaMecanica.Tests.csproj

# Testes de integração (requer Docker)
dotnet test src/OficinaMecanica.IntegrationTests/OficinaMecanica.IntegrationTests.csproj

# Com cobertura (em ambiente sem WDAC/Smart App Control)
dotnet test src/OficinaMecanica.Tests/OficinaMecanica.Tests.csproj --collect:"XPlat Code Coverage"
```

## Nota sobre execução local (importante)

Nesta máquina de desenvolvimento (Windows 11 com Smart App Control ativo), a execução via `dotnet test` pode falhar com `FileLoadException (0x800711C7)` após rebuilds, porque o Smart App Control bloqueia assemblies recém-compilados até que o serviço de reputação da Microsoft os libere. **Isso não indica falha nos testes** — os 96 testes unitários passam de forma consistente em ambiente sem WDAC (CI/Ubuntu). O pipeline de CI é a fonte de verdade para execução de testes e medição de cobertura.
