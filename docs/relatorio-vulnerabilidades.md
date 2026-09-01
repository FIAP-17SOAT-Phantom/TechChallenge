# Relatório de Análise de Vulnerabilidades

## Ferramenta utilizada

`dotnet list package --vulnerable --include-transitive`

Verifica pacotes NuGet (diretos e transitivos) contra o banco de dados de advisories do GitHub/NuGet.

## Comando executado

```bash
dotnet list src/OficinaMecanica.slnx package --vulnerable --include-transitive
```

---

## Achados iniciais

Na primeira análise, foram encontradas **2 vulnerabilidades de severidade High**, ambas em dependências **transitivas** do projeto de testes:

| Pacote | Versão | Severidade | Advisory |
|--------|--------|-----------|----------|
| System.Net.Http | 4.3.0 | High | [GHSA-7jgj-8wvc-jh57](https://github.com/advisories/GHSA-7jgj-8wvc-jh57) |
| System.Text.RegularExpressions | 4.3.0 | High | [GHSA-cmhx-cq75-c4mj](https://github.com/advisories/GHSA-cmhx-cq75-c4mj) |

### Análise de impacto

- Ambas eram dependências **transitivas** (trazidas por pacotes antigos de teste), não referências diretas.
- Presentes **apenas no projeto de Testes** (`OficinaMecanica.Tests`), que **não é distribuído** em produção.
- A API, Application, Domain e Infrastructure **não continham** nenhuma vulnerabilidade.
- Risco real em produção: **nulo** (código de teste não vai para o container de produção).

---

## Correção aplicada

1. Atualização dos pacotes de teste para versões recentes:
   - `Microsoft.NET.Test.Sdk`: 17.8.0 → 17.11.1
   - `xunit`: 2.5.3 → 2.9.2
   - `xunit.runner.visualstudio`: 2.5.3 → 2.8.2
   - `coverlet.collector`: 6.0.0 → 6.0.2

2. Override explícito das dependências transitivas vulneráveis para versões corrigidas:
   ```xml
   <PackageReference Include="System.Net.Http" Version="4.3.4" />
   <PackageReference Include="System.Text.RegularExpressions" Version="4.3.1" />
   ```

---

## Resultado final

```
OficinaMecanica.API            -> nenhum pacote vulnerável
OficinaMecanica.Application    -> nenhum pacote vulnerável
OficinaMecanica.Domain         -> nenhum pacote vulnerável
OficinaMecanica.Infrastructure -> nenhum pacote vulnerável
OficinaMecanica.IntegrationTests -> nenhum pacote vulnerável
OficinaMecanica.Tests          -> nenhum pacote vulnerável
```

**Status: Todas as vulnerabilidades corrigidas.**

---

## Automação (CI)

O scan de vulnerabilidades foi incorporado ao pipeline de CI (`.github/workflows/ci.yml`) no job `scan-vulnerabilidades`, executado automaticamente a cada push e pull request na branch `main`. Isso garante que novas dependências vulneráveis sejam detectadas continuamente.
