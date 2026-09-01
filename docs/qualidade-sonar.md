# Qualidade de Codigo com Sonar

## Estado atual

O projeto possui build sem avisos, testes unitarios e de integracao, cobertura com coverlet, relatorio pelo ReportGenerator e scan de dependencias NuGet no GitHub Actions.

O projeto SonarQube Cloud foi identificado como `FIAP-17SOAT-Phantom_TechChallenge`, na organizacao `fiap-17soat-phantom`, e o workflow `.github/workflows/build.yml` foi preparado. Este documento ainda nao publica numeros de bugs, code smells, duplicacao ou Quality Gate antes da primeira execucao real no GitHub.

## Metricas que devem ser coletadas

Depois da primeira analise, registrar no README e no relatorio final:

| Metrica | Evidencia esperada |
|---------|--------------------|
| Quality Gate | Passed ou justificativa dos criterios reprovados |
| Bugs | Quantidade e severidade |
| Vulnerabilidades | Quantidade e severidade |
| Security Hotspots | Quantidade e percentual revisado |
| Code Smells | Quantidade e principais categorias |
| Divida tecnica | Tempo estimado pelo Sonar |
| Duplicacao | Percentual de linhas duplicadas |
| Cobertura | Percentual importado dos testes |
| Reliability Rating | Classificacao obtida |
| Security Rating | Classificacao obtida |
| Maintainability Rating | Classificacao obtida |

## SonarCloud

Para um repositorio hospedado no GitHub, o SonarCloud analisa pushes e pull requests pelo workflow dedicado. A configuracao utiliza:

- organizacao `fiap-17soat-phantom`;
- chave `FIAP-17SOAT-Phantom_TechChallenge`;
- token armazenado como secret `SONAR_TOKEN` do GitHub;
- permissao do Sonar para acessar o repositorio.

Configuracao no GitHub:

```text
Secret:   SONAR_TOKEN
```

O token nunca deve ser colocado no README, workflow, `.env.example` ou historico Git.

O workflow executa build Release, os 96 testes unitarios e gera `coverage.opencover.xml` com `coverlet.msbuild`. O scanner recebe esse relatorio pela propriedade `sonar.cs.opencover.reportsPaths`, permitindo que o dashboard apresente cobertura C# em vez de apenas analisar o codigo estaticamente.

A cobertura do Quality Gate foi delimitada ao projeto Domain, que apresentou `82,03%` de linhas na validacao OpenCover. API, Application e Infrastructure continuam sendo analisadas pelo Sonar para bugs, vulnerabilidades, hotspots, duplicacao e code smells, mas foram explicitamente excluidas do calculo de cobertura porque a meta academica definida para esta fase se refere aos dominios criticos. Essa delimitacao deve permanecer declarada no README e no relatorio, evitando apresentar o percentual como cobertura global da solucao.

Fluxo oficial do scanner para .NET:

```bash
dotnet tool install --global dotnet-sonarscanner
dotnet sonarscanner begin /k:"PROJECT_KEY" /o:"ORGANIZATION" /d:sonar.token="$SONAR_TOKEN"
dotnet build src/OficinaMecanica.slnx --no-incremental
dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"
```

O token precisa ser informado tanto no `begin` quanto no `end`. No GitHub Actions, o checkout deve usar historico completo (`fetch-depth: 0`) para melhorar a relevancia da analise.

Documentacao oficial:

- [SonarScanner for .NET](https://docs.sonarsource.com/sonarqube-cloud/analyzing-source-code/scanners/sonarscanner-for-dotnet/using);
- [SonarQube Cloud com GitHub Actions](https://docs.sonarsource.com/sonarcloud/advanced-setup/ci-based-analysis/github-actions-for-sonarcloud).

## SonarQube local

Como alternativa, a equipe pode executar um servidor SonarQube local ou institucional. Nesse caso, o scanner precisa receber URL, token e chave do projeto. O resultado local deve ser exportado por capturas ou relatorio para permanecer verificavel na entrega.

## Quality Gate recomendado

Uma politica coerente para o Tech Challenge e aplicar o gate ao codigo novo:

- nenhuma vulnerabilidade nova;
- nenhum bug novo de alta severidade;
- Security Hotspots revisados;
- cobertura minima de 80% nos dominios criticos;
- duplicacao controlada;
- ratings de confiabilidade, seguranca e manutenibilidade registrados.

Os limites definitivos devem seguir o criterio da disciplina e a configuracao real do Sonar escolhido.

## Evidencias para a entrega

Depois da primeira analise bem-sucedida:

1. guardar o link do dashboard;
2. adicionar badge do Quality Gate ao README;
3. adicionar badge de cobertura, se publicado;
4. registrar data e commit analisado;
5. documentar achados criticos ou altos;
6. corrigir ou justificar cada achado relevante;
7. confirmar o job verde no GitHub Actions.

Enquanto o workflow ainda nao tiver uma execucao verde e um dashboard consultado, as metricas devem ser descritas como pendentes, e nao como aprovadas.
