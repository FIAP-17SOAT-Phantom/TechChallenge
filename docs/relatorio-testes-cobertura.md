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

## Comando padrão de teste

```bash
# Apenas executar testes
dotnet test src/OficinaMecanica.slnx

# Com cobertura (em ambiente sem WDAC)
dotnet test src/OficinaMecanica.slnx --collect:"XPlat Code Coverage"
```
