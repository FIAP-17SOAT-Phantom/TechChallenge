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

O workflow ja possui execucao verde e o dashboard foi consultado em 01/09/2026. A primeira medicao apresentou Quality Gate aprovado, cobertura de 77,8%, duplicacao de 3,4%, 48 issues abertas e Security Rating C. Esses valores formam a linha de base anterior a rodada de correcoes; uma nova execucao deve ser usada para registrar os resultados finais.

## Primeira execucao no GitHub

A primeira execucao confirmou que checkout, scanner, autenticacao, build, testes e geracao do OpenCover funcionaram. O envio final foi recusado porque o projeto ainda estava com `Automatic Analysis` habilitado no SonarQube Cloud, ao mesmo tempo em que o GitHub Actions executava a analise por CI.

Para utilizar o workflow e importar cobertura:

```text
SonarQube Cloud
    -> projeto FIAP-17SOAT-Phantom_TechChallenge
    -> Administration
    -> Analysis Method
    -> Automatic Analysis
    -> desmarcar Enabled for this project
```

Depois, execute novamente o workflow `SonarQube Cloud` no GitHub Actions. Analise automatica e analise por CI sao alternativas mutuamente exclusivas. A analise por CI foi escolhida neste projeto porque permite compilar o .NET, executar testes e importar cobertura OpenCover.

## Tratamento dos achados da primeira analise

Os achados de maior impacto foram tratados no codigo e na infraestrutura: credencial fixa do PostgreSQL removida, container configurado com usuario nao root, regex protegidas contra execucao excessiva, contratos HTTP de tipos valor marcados como obrigatorios e metodos de maior complexidade divididos em operacoes menores.

Tambem foram corrigidos apontamentos de confiabilidade e manutencao, incluindo `RunAsync`, `AddAuthorizationBuilder`, tipos concretos para colecoes privadas, literal repetido e ternario aninhado. As migrations do Entity Framework foram excluidas da analise por serem codigo gerado automaticamente.

Antes de apresentar as metricas como resultado final, deve-se enviar estas alteracoes ao GitHub e confirmar no novo dashboard:

- reducao das 48 issues;
- melhoria do Security Rating C;
- permanencia do Quality Gate aprovado;
- cobertura e duplicacao atualizadas.

## Resultado apos as correcoes

O dashboard atualizado em 01/09/2026 apresentou:

| Metrica | Resultado |
|---------|-----------|
| Quality Gate | Passed |
| Issues abertas | 2 |
| Security Rating | A |
| Security issues | 0 |
| Duplicacao | 0,0% |
| Cobertura | 77,8% |

Em comparacao com a linha de base, foram encerradas 46 das 48 issues, todas as 9 security issues foram eliminadas, o Security Rating evoluiu de C para A e a duplicacao caiu de 3,4% para 0,0%. Restam duas issues de qualidade a serem classificadas pela equipe na tela de analise detalhada.

### Quality Gate do codigo novo

Na aba `Analysis > Summary`, o recorte de codigo novo apresentou 0 novas issues, 0 issues aceitas, 83,33% de cobertura, 0,0% de duplicacao e 0 security hotspots. A cobertura supera o requisito de 80% configurado pelo Quality Gate. Assim, as duas issues do codigo geral sao anteriores ao periodo de New Code e nao comprometem o gate da entrega atual.
