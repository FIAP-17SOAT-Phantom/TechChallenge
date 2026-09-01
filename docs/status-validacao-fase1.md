# Status de Validacao da Fase 1

Data da auditoria: 01/09/2026.

## Resultado executivo

O codigo funcional da API esta implementado e o ambiente Docker Compose funciona com PostgreSQL real. O build, os testes unitarios, o scan de pacotes, o Swagger e o login no ambiente Compose foram validados.

Durante a auditoria foi encontrada e corrigida uma falha na inicializacao dos testes de integracao. Depois da correcao do fixture e do tratamento global de validacao, os sete cenarios passaram contra PostgreSQL 16 real provisionado pelo Testcontainers.

## Evidencias executadas

| Verificacao | Resultado |
|-------------|-----------|
| Build Release de `src/OficinaMecanica.slnx` | Aprovado, 0 erros e 0 avisos |
| Testes unitarios | 96 aprovados, 0 falhas |
| Docker Compose | Arquivo valido |
| Build da imagem da API | Aprovado, 0 erros e 0 avisos |
| Container PostgreSQL | Em execucao e `healthy` |
| Container API | Em execucao na porta 8080 |
| Swagger/OpenAPI | HTTP 200 |
| Login administrativo | HTTP 200 e JWT emitido |
| Scan NuGet atualizado | Nenhum pacote vulneravel nos seis projetos |
| Testes de integracao | 7 aprovados, 0 falhas, com PostgreSQL 16 real |

## Correcoes realizadas nos testes de integracao

Arquivo envolvido:

```text
src/OficinaMecanica.IntegrationTests/CustomWebApplicationFactory.cs
```

O fixture adicionava `Jwt:Secret` somente com `ConfigureAppConfiguration`, mas `Program.cs` consultava essa chave antes da configuracao estar disponivel:

```text
WebApplicationFactory inicia Program.cs
    -> Program.cs cria WebApplicationBuilder
    -> Program.cs consulta Jwt:Secret
    -> configuracao de teste ainda nao esta disponivel nesse ponto
    -> InvalidOperationException: Jwt:Secret nao configurado
```

O fixture agora inicia o PostgreSQL antes da API, disponibiliza connection string, JWT e seed por variaveis de ambiente antes do `Program.cs` e restaura o ambiente ao ser descartado. As tres classes usam uma unica collection fixture sem paralelismo, impedindo disputa por configuracoes globais e evitando a criacao desnecessaria de um banco por classe.

Quando os testes passaram a iniciar, dois cenarios revelaram que `GlobalExceptionHandler` reconhecia `ValidationException`, mas nao definia `HttpResponse.StatusCode` antes de escrever o `ProblemDetails`. A API retornava 500 para entradas invalidas. O handler foi corrigido para definir o status 400 nas validacoes e o status correspondente nas demais excecoes.

## Cobertura atual

O relatorio existente registra 96 testes unitarios e cobertura superior a 80% nas classes criticas de Domain. Nesta maquina, a reinstrumentacao local pelo coverlet pode ser bloqueada pelo Windows Smart App Control. O pipeline Ubuntu permanece como ambiente recomendado para produzir o artefato de cobertura reproduzivel.

Durante esta auditoria, os 96 testes foram confirmados, mas a porcentagem nao foi recalculada localmente.

## Cobertura de integracao existente

Os testes atuais foram escritos para:

- login valido;
- senha incorreta;
- email invalido;
- rota protegida sem token;
- rota protegida com token invalido;
- criacao e consulta de servico no PostgreSQL;
- validacao de servico invalido.

Ainda e recomendavel ampliar a integracao para o fluxo principal exigido na demonstracao:

```text
Cliente
    -> Veiculo
    -> Ordem de Servico
    -> Diagnostico
    -> Orcamento
    -> Envio
    -> Aprovacao
    -> Reserva de estoque
    -> Execucao
    -> Finalizacao
    -> Entrega
```

Tambem devem ser priorizados os cenarios transacionais de estoque insuficiente, cancelamento com liberacao de reserva e autorizacao da area do cliente.

## Qualidade e seguranca

O comando abaixo foi executado com acesso atualizado ao NuGet:

```powershell
dotnet list src/OficinaMecanica.slnx package --vulnerable --include-transitive
```

Nenhum pacote vulneravel foi encontrado em API, Application, Domain, Infrastructure, IntegrationTests ou Tests.

O pipeline `.github/workflows/ci.yml` possui jobs para build, testes unitarios com cobertura, testes de integracao e scan de vulnerabilidades. Ainda e necessario confirmar no GitHub que todos os jobs passam depois destas correcoes.

## Documentacao e entrega academica

Ja existem no repositorio:

- README de execucao;
- decisoes de arquitetura, banco e stack;
- modelagem DDD;
- arquivos CSV e instrucoes para importacao do Event Storming no Miro;
- evolucao completa do backend;
- guia de API, Docker e PostgreSQL;
- roadmap do login e JWT;
- relatorio de testes e cobertura;
- relatorio de vulnerabilidades.

Itens que dependem de confirmacao ou acao externa ao codigo:

- importar e organizar o board definitivo no Miro;
- confirmar repositorio privado e acesso ao usuario exigido pela FIAP;
- executar e registrar um fluxo manual completo pelo Swagger;
- confirmar o pipeline verde no GitHub;
- gravar o video de ate 15 minutos;
- preparar o PDF final com participantes e links.

## Ordem recomendada para finalizar

1. Adicionar integracao do fluxo completo da Ordem de Servico e estoque.
2. Confirmar cobertura e jobs do CI no GitHub.
3. Demonstrar manualmente o fluxo completo pelo Swagger.
4. Finalizar Miro, video e PDF da entrega.

## Classificacao atual

| Area | Estado |
|------|--------|
| Codigo funcional da API | Concluido |
| PostgreSQL, migrations e Docker | Concluido e validado |
| Testes unitarios | Concluidos e validados |
| Cobertura critica | Documentada; confirmar artefato atual do CI |
| Testes de integracao | 7 cenarios implementados e validados com PostgreSQL real |
| Scan de pacotes | Concluido e validado sem vulnerabilidades |
| Fluxo manual completo | Pendente de demonstracao registrada |
| Entregaveis academicos externos | Parcialmente pendentes |
