# Evolucao da Implementacao do Backend

## Objetivo

Este documento registra, de forma incremental, as alteracoes realizadas para conectar e completar o backend do Sistema Integrado de Atendimento e Execucao de Servicos da Oficina Mecanica.

As implementacoes preservam a arquitetura existente:

```text
API
  -> Application
    -> Domain

Infrastructure
  -> Application / Domain
```

O projeto continua utilizando Clean Architecture, DDD, CQRS, MediatR, Repository Pattern, Unit of Work, Domain Events, FluentValidation, Entity Framework Core e PostgreSQL.

### Convencao visual adicional

Por decisao da equipe, assinaturas de metodos, construtores, records e chamadas curtas devem permanecer em uma unica linha, mesmo quando possuem varios parametros. Todo codigo criado a partir do Bloco 4 deve seguir essa convencao, e o codigo adicionado nos blocos anteriores foi ajustado retroativamente.

---

## Bloco 1 - Integracao da API, PostgreSQL, Migrations e Docker

### Estado encontrado

- A API ainda utilizava o endpoint `WeatherForecast` do template do ASP.NET Core.
- `Application` e `Infrastructure` ja possuiam metodos de extensao para Dependency Injection, mas nao eram registrados pela API.
- Nao existiam Controllers.
- Nao existia connection string nos arquivos de configuracao da API.
- Nao existiam migrations do Entity Framework Core.
- O arquivo `docker-compose.yml` possuia indentacao YAML invalida.
- O build da solucao passava sem erros.
- O projeto de testes continha apenas o teste criado pelo template.

### Alteracoes realizadas

O `Program.cs` passou a registrar:

```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

O pipeline HTTP passou a mapear Controllers:

```csharp
app.UseAuthorization();
app.MapControllers();
```

O endpoint `WeatherForecast` e os tipos associados ao template foram removidos.

Foi adicionada a connection string local do PostgreSQL em `appsettings.json`. Em containers, ela e substituida pela variavel de ambiente `ConnectionStrings__DefaultConnection`.

O registro do `AppDbContext` foi mantido na Infrastructure e teve apenas a formatacao corrigida. O provider continua sendo Npgsql e as migrations continuam pertencendo ao assembly da Infrastructure.

### Entity Framework Core

Foi adicionada uma referencia privada a `Microsoft.EntityFrameworkCore.Design` no projeto da API para permitir que a API seja utilizada como startup project pelas ferramentas do EF Core.

A ferramenta `dotnet-ef` versao 8.0.11 foi registrada em `.config/dotnet-tools.json`. Com isso, a equipe pode restaurar a ferramenta executando:

```bash
dotnet tool restore
```

Foi criada a migration inicial `InitialCreate` dentro de:

```text
src/OficinaMecanica.Infrastructure/Persistence/Migrations
```

### Docker Compose

O Compose foi corrigido para subir:

- API ASP.NET Core na porta `8080`;
- PostgreSQL 16 Alpine na porta `5432`;
- volume persistente `postgres_data`;
- healthcheck do PostgreSQL;
- dependencia da API pelo banco saudavel;
- connection string apropriada para o hostname `db`.

O ambiente utilizado durante a implementacao nao possui Docker instalado. Por esse motivo, o arquivo foi validado estruturalmente, mas os containers ainda nao foram executados neste ambiente.

### Arquivos principais alterados

- `src/OficinaMecanica.API/Program.cs`
- `src/OficinaMecanica.API/OficinaMecanica.API.csproj`
- `src/OficinaMecanica.API/appsettings.json`
- `src/OficinaMecanica.Infrastructure/DependencyInjection.cs`
- `docker-compose.yml`

### Arquivos criados

- `.config/dotnet-tools.json`
- migration `InitialCreate`;
- designer da migration;
- snapshot do modelo do `AppDbContext`.

---

## Bloco 2 - Cliente e Veiculo

### Cliente

Foram implementados os casos de uso para:

- criar cliente;
- consultar cliente por identificador;
- listar clientes;
- atualizar cliente;
- excluir cliente.

O fluxo utiliza:

```text
ClientesController
  -> Command / Query
    -> MediatR
      -> Handler
        -> Repository Interface
          -> Repository EF Core
```

O CPF continua sendo criado e validado pelo Value Object `Cpf`. O e-mail continua sendo criado e validado pelo Value Object `Email`. O handler de criacao verifica duplicidade de CPF antes de persistir o aggregate.

O metodo `Cliente.Atualizar` foi reforcado para impedir telefone vazio, preservando no Domain a mesma invariante exigida na criacao.

A exclusao e bloqueada quando o cliente possui veiculos ou ordens de servico vinculadas.

### Veiculo

Foram implementados os casos de uso para:

- criar veiculo;
- consultar veiculo por identificador;
- listar veiculos por cliente;
- atualizar veiculo;
- excluir veiculo.

A placa continua sendo criada e validada pelo Value Object `Placa`. O handler verifica a existencia do cliente e impede placas duplicadas.

O metodo `Veiculo.Atualizar` foi reforcado para preservar as invariantes de marca, modelo e ano que ja eram aplicadas pelo construtor.

A exclusao e bloqueada quando o veiculo possui alguma ordem de servico vinculada. Para essa verificacao, foi adicionada a operacao `ExistsByVeiculoIdAsync` ao contrato e ao repositorio de ordens de servico.

### Politica de exclusao

O modelo atual nao possui propriedade `Ativo` em Cliente ou Veiculo e nao define uma politica de inativacao. Para evitar uma alteracao arbitraria de regra e schema, foi mantida a exclusao fisica ja suportada pelos repositorios.

Essa decisao deve ser revista caso o negocio determine que o historico de clientes e veiculos nunca pode ser removido.

### Endpoints adicionados

| Metodo | Rota | Descricao |
|--------|------|-----------|
| POST | `/api/clientes` | Cria um cliente |
| GET | `/api/clientes` | Lista os clientes |
| GET | `/api/clientes/{clienteId}` | Consulta um cliente |
| PUT | `/api/clientes/{clienteId}` | Atualiza um cliente |
| DELETE | `/api/clientes/{clienteId}` | Exclui um cliente sem vinculos |
| POST | `/api/veiculos` | Cria um veiculo |
| GET | `/api/veiculos?clienteId={id}` | Lista os veiculos de um cliente |
| GET | `/api/veiculos/{veiculoId}` | Consulta um veiculo |
| PUT | `/api/veiculos/{veiculoId}` | Atualiza um veiculo |
| DELETE | `/api/veiculos/{veiculoId}` | Exclui um veiculo sem OS vinculada |

### Organizacao dos casos de uso

Os arquivos foram criados nas pastas existentes:

```text
OficinaMecanica.Application/UseCases/Atendimento
  Commands/
  Queries/
```

Foram mantidos arquivos separados para Commands, Queries, Handlers, Validators e DTOs.

Os Controllers foram criados em:

```text
OficinaMecanica.API/Controllers
```

### Pendencias conhecidas

- FluentValidation ainda precisa ser integrado ao tratamento global de excecoes com ProblemDetails.
- Os Controllers ainda utilizam respostas de erro locais e simples. Isso sera centralizado no bloco de tratamento global.
- JWT e autorizacao por roles ainda nao foram implementados.
- Ainda nao existem testes reais para os novos casos de uso e endpoints.
- A execucao real do Docker Compose continua pendente por indisponibilidade do Docker no ambiente atual.

---

## Validacoes executadas

Apos cada bloco foram executados:

```bash
dotnet build src/OficinaMecanica.slnx
dotnet test src/OficinaMecanica.slnx
git diff --check
```

Resultados ate este ponto:

- build concluido sem erros;
- build concluido sem avisos de compilacao;
- teste placeholder aprovado;
- nenhuma inconsistencia detectada por `git diff --check`.

---

## Proximos blocos

1. CRUD de Servicos.
2. CRUD de Pecas e operacoes de estoque.
3. Fluxo completo da Ordem de Servico.
4. Fluxo completo de Orcamento.
5. Atomicidade da aprovacao do orcamento e reserva de estoque.
6. Administrativo.
7. JWT e autorizacao.
8. Tratamento global de excecoes com ProblemDetails.
9. Swagger completo.
10. Testes unitarios e de integracao.

---

## Bloco 3 - Catalogo de Servicos

### Implementacao

Foram adicionados os casos de uso para:

- criar servico;
- consultar servico por identificador;
- listar servicos, com opcao de retornar somente os ativos;
- atualizar servico;
- desativar servico;
- reativar servico.

O aggregate `Servico` ja possuia a propriedade `Ativo` e os comportamentos `Desativar` e `Ativar`. Por isso, a operacao HTTP de exclusao foi implementada como desativacao logica, preservando o servico e seu historico.

O metodo `Servico.Atualizar` foi reforcado para aplicar as mesmas invariantes do construtor:

- nome obrigatorio;
- preco base nao negativo;
- tempo estimado maior que zero.

### Endpoints adicionados

| Metodo | Rota | Descricao |
|--------|------|-----------|
| POST | `/api/servicos` | Cria um servico |
| GET | `/api/servicos/{servicoId}` | Consulta um servico |
| GET | `/api/servicos?somenteAtivos=true` | Lista os servicos |
| PUT | `/api/servicos/{servicoId}` | Atualiza um servico |
| DELETE | `/api/servicos/{servicoId}` | Desativa um servico |
| PATCH | `/api/servicos/{servicoId}/ativar` | Reativa um servico |

### Organizacao

Os Commands, Queries, Handlers, Validators e DTO foram adicionados em:

```text
OficinaMecanica.Application/UseCases/CatalogoServicos
  Commands/
  Queries/
```

O `ServicosController` apenas traduz HTTP e encaminha requests ao MediatR. As regras de atualizacao e ativacao permanecem no aggregate.

---

## Bloco 4 - Pecas e Entrada de Estoque

### Implementacao

Foram adicionados os casos de uso para:

- criar peca;
- consultar peca por identificador;
- listar todas as pecas;
- filtrar pecas com estoque baixo;
- atualizar dados comerciais e quantidade minima;
- adicionar unidades ao estoque;
- excluir peca sem historico de reservas.

O DTO de peca apresenta separadamente:

- quantidade fisica em estoque;
- quantidade reservada;
- quantidade disponivel;
- quantidade minima.

O codigo da peca e normalizado com `Trim` e `ToUpperInvariant` durante a criacao. O handler impede o cadastro de codigos duplicados utilizando o repositorio existente.

### Regras mantidas no Domain

O aggregate `Peca` continua sendo responsavel por:

- calcular a quantidade disponivel;
- validar e adicionar estoque;
- reservar unidades;
- liberar reservas;
- consumir reservas;
- emitir `EstoqueBaixoEvent` quando a disponibilidade atinge o minimo configurado.

O construtor passou a exigir codigo nao vazio. O metodo `Atualizar` passou a preservar as mesmas invariantes de nome, preco e quantidade minima aplicadas na criacao.

### Exclusao e rastreabilidade

A configuracao do EF Core utiliza cascade entre Peca e Reserva. Para impedir que uma exclusao apague historico de movimentacao, o caso de uso bloqueia a remocao de qualquer peca que ja possua reservas, independentemente do status delas.

O modelo atual nao possui propriedade `Ativo` em Peca. Caso o negocio exija retirada de itens do catalogo sem exclusao fisica, devera ser adicionada posteriormente uma politica explicita de inativacao.

### Reserva e consumo

Nao foram criados endpoints administrativos para reservar, liberar ou consumir estoque de forma desconectada. Essas operacoes representam etapas do fluxo de Orcamento e Ordem de Servico e serao orquestradas nos respectivos casos de uso, com consistencia transacional.

### Endpoints adicionados

| Metodo | Rota | Descricao |
|--------|------|-----------|
| POST | `/api/pecas` | Cria uma peca |
| GET | `/api/pecas/{pecaId}` | Consulta uma peca |
| GET | `/api/pecas?somenteEstoqueBaixo=false` | Lista ou filtra pecas com estoque baixo |
| PUT | `/api/pecas/{pecaId}` | Atualiza os dados da peca |
| PATCH | `/api/pecas/{pecaId}/estoque` | Adiciona unidades ao estoque |
| DELETE | `/api/pecas/{pecaId}` | Exclui uma peca sem reservas |

### Organizacao

Os Commands, Queries, Handlers, Validators e DTO foram adicionados em:

```text
OficinaMecanica.Application/UseCases/Estoque
  Commands/
  Queries/
```

O `PecasController` apenas recebe dados HTTP, cria requests e os envia pelo MediatR.

---

## Bloco 5 - Fluxo da Ordem de Servico

### Implementacao

Foram adicionados casos de uso para:

- criar Ordem de Servico;
- consultar uma OS com seus itens;
- listar OS com filtros basicos e paginacao;
- iniciar diagnostico;
- registrar diagnostico e itens identificados;
- finalizar OS;
- registrar entrega;
- cancelar OS.

Todas as transicoes chamam os comportamentos existentes do aggregate `OrdemDeServico`. Os handlers nao alteram diretamente a propriedade `Status` e nao reproduzem a maquina de estados na Application.

### Maquina de estados preservada

```text
Recebida
  -> EmDiagnostico
    -> AguardandoAprovacao
      -> EmExecucao
        -> Finalizada
          -> Entregue
```

O cancelamento continua sendo permitido somente conforme `OrdemDeServico.Cancelar`.

### Diagnostico

Ao registrar o diagnostico, o handler confirma:

- existencia da OS;
- existencia e ativacao de cada servico;
- existencia das pecas informadas;
- quantidade positiva dos itens;
- diagnostico e lista de itens obrigatorios.

Depois dessas verificacoes, a alteracao de diagnostico, itens e status e executada pelo aggregate.

O `ItemOS` passou a proteger as invariantes de identificador de servico obrigatorio e quantidade maior que zero.

### Consulta e listagem

O detalhamento da OS agora inclui:

- identificadores de cliente, veiculo, mecanico e orcamento;
- numero e status;
- datas de abertura e finalizacao;
- diagnostico;
- itens de servico e peca.

A listagem aceita cliente, status, pagina e tamanho de pagina. A validacao limita o tamanho da pagina a 100 registros.

### Endpoints adicionados

| Metodo | Rota | Descricao |
|--------|------|-----------|
| POST | `/api/ordens-de-servico` | Cria uma OS |
| GET | `/api/ordens-de-servico/{ordemDeServicoId}` | Consulta uma OS |
| GET | `/api/ordens-de-servico` | Lista OS com filtros |
| PATCH | `/api/ordens-de-servico/{id}/iniciar-diagnostico` | Inicia o diagnostico |
| PATCH | `/api/ordens-de-servico/{id}/registrar-diagnostico` | Registra diagnostico e itens |
| PATCH | `/api/ordens-de-servico/{id}/finalizar` | Finaliza a OS |
| PATCH | `/api/ordens-de-servico/{id}/entregar` | Registra a entrega |
| PATCH | `/api/ordens-de-servico/{id}/cancelar` | Cancela a OS em estado permitido |

### Pendencias transacionais explicitas

O fluxo ainda depende do bloco de Orcamento e Estoque para ficar completo:

- gerar e vincular o orcamento;
- aprovar o orcamento e iniciar a execucao na mesma transacao da reserva;
- consumir as reservas antes de concluir a finalizacao;
- liberar reservas quando uma OS em execucao for cancelada.

Os endpoints de finalizar e cancelar ja respeitam a maquina de estados, mas seus handlers serao ampliados para coordenar estoque quando as reservas estiverem conectadas. Essa pendencia nao deve ser considerada fluxo final de producao.

---

## Bloco 6 - Orcamento e Consistencia do Estoque

### Casos de uso implementados

Foram adicionados casos de uso para:

- gerar orcamento a partir do diagnostico da OS;
- consultar orcamento por identificador;
- consultar a versao atual por Ordem de Servico;
- enviar orcamento;
- aprovar orcamento;
- rejeitar orcamento;
- gerar nova versao depois de rejeicao ou cancelamento.

Na geracao, os itens da OS sao convertidos em itens de orcamento usando os precos atuais do catalogo de servicos e das pecas. O valor total continua sendo calculado pelo aggregate `Orcamento` a partir de seus itens.

Cada nova versao recebe o proximo numero disponivel e substitui o identificador de orcamento vinculado na OS. Uma nova versao somente pode ser criada quando a anterior estiver rejeitada ou cancelada.

### Correcao da aprovacao

O desenho anterior executava:

```text
aprovar orcamento
  -> SaveChanges
    -> publicar OrcamentoAprovadoEvent
      -> reservar estoque
      -> iniciar OS
      -> novo SaveChanges
```

O `AppDbContext` ainda capturava qualquer excecao do handler do evento e apenas registrava o erro. Isso permitia persistir o orcamento como aprovado mesmo quando a reserva falhava.

O handler operacional de `OrcamentoAprovadoEvent` foi removido. O evento continua sendo emitido pelo Domain como registro do fato ocorrido, mas nao e mais responsavel pelas alteracoes que precisam de consistencia forte.

A aprovacao agora executa:

```text
carregar orcamento enviado
  -> carregar OS aguardando aprovacao
  -> agrupar quantidades por peca
  -> carregar e validar todo o estoque
  -> criar as reservas
  -> aprovar o orcamento
  -> iniciar a execucao da OS
  -> um unico SaveChangesAsync
```

Se qualquer validacao falhar antes da persistencia, nenhum `SaveChangesAsync` e executado. Se a persistencia falhar, a transacao interna do EF Core reverte todas as alteracoes daquele SaveChanges.

Itens repetidos da mesma peca sao agrupados antes da verificacao. Isso impede validar cada linha isoladamente contra a mesma quantidade disponivel.

Todo o caso de uso, incluindo as leituras de disponibilidade, e executado dentro de uma transacao PostgreSQL com isolamento `Serializable`. Essa protecao impede que duas aprovacoes concorrentes confirmem a mesma disponibilidade. Em caso de conflito de serializacao, uma das transacoes falha e sofre rollback, em vez de aprovar dois orcamentos sobre o mesmo estoque.

O contrato transacional foi adicionado a `IUnitOfWork`, enquanto a implementacao com EF Core permanece no `AppDbContext`. A Application nao passou a depender de EF Core ou PostgreSQL.

### Finalizacao e consumo

Ao finalizar uma OS, o handler agora:

- exige um orcamento aprovado vinculado;
- localiza todas as pecas do orcamento;
- confirma a existencia das reservas ativas;
- consome as reservas no aggregate `Peca`;
- reduz estoque fisico e reservado;
- finaliza a OS;
- persiste todas as alteracoes uma unica vez.

### Cancelamento e liberacao

Ao cancelar uma OS em execucao, o handler:

- carrega o orcamento aprovado;
- localiza as reservas ativas da OS;
- libera cada reserva no aggregate `Peca`;
- reduz a quantidade reservada sem reduzir estoque fisico;
- cancela a OS;
- persiste todas as alteracoes uma unica vez.

Para cancelamentos anteriores a execucao nao existem reservas a liberar.

### Endpoints adicionados

| Metodo | Rota | Descricao |
|--------|------|-----------|
| POST | `/api/orcamentos` | Gera um orcamento ou nova versao |
| GET | `/api/orcamentos/{orcamentoId}` | Consulta um orcamento |
| GET | `/api/orcamentos/ordem-de-servico/{ordemDeServicoId}` | Consulta a versao atual da OS |
| PATCH | `/api/orcamentos/{orcamentoId}/enviar` | Envia o orcamento |
| PATCH | `/api/orcamentos/{orcamentoId}/aprovar` | Aprova, reserva estoque e inicia a OS |
| PATCH | `/api/orcamentos/{orcamentoId}/rejeitar` | Rejeita o orcamento |

### Decisao sobre Domain Events

`OrcamentoAprovadoEvent` foi preservado porque a aprovacao e um fato relevante para notificacoes, auditoria ou integracoes futuras. Ele nao deve executar etapas que precisam pertencer a mesma transacao da aprovacao.

Essa separacao segue a regra:

```text
consistencia obrigatoria do caso de uso
  -> orquestracao antes do commit

efeitos secundarios tolerantes a falha
  -> Domain Event depois do commit
```

---

## Bloco 7 - JWT, Roles, ProblemDetails e Swagger

### ASP.NET Core Identity

A decisao tecnica existente especificava ASP.NET Core Identity simplificado, login por email e senha e as roles `Admin`, `Atendente`, `Mecanico` e `Cliente`. Como ainda nao existia um modelo de credenciais, o Identity foi implementado na Infrastructure, sem adicionar senha ou dependencia de seguranca ao Domain.

O `AppDbContext` passou a herdar de `IdentityDbContext<UsuarioSistema>`. O usuario de autenticacao pode possuir um `ClienteId` quando representa um cliente da oficina.

Foi criada a migration `AddIdentity`, contendo as tabelas e relacionamentos do Identity.

No startup da API:

- migrations pendentes sao aplicadas automaticamente;
- as quatro roles sao criadas quando ainda nao existem;
- usuarios iniciais configurados em `Authentication:SeedUsers` sao criados;
- nenhuma senha inicial fica gravada no codigo-fonte ou no `appsettings.json`.

### Configuracao segura

O segredo JWT deve ser fornecido externamente pela chave:

```text
Jwt__Secret
```

Ele deve possuir pelo menos 32 bytes. A aplicacao interrompe a inicializacao quando o segredo esta ausente ou e curto demais.

O Docker Compose exige:

```text
JWT_SECRET
ADMIN_PASSWORD
```

Opcionalmente, `ADMIN_EMAIL` pode substituir `admin@oficina.local`. Um modelo foi adicionado em `.env.example`; o arquivo `.env` real nao deve ser versionado.

Exemplo de preparacao local:

```bash
cp .env.example .env
```

Depois, os valores de exemplo devem ser substituidos antes de executar:

```bash
docker compose up --build
```

### Autenticacao e usuarios

Foram adicionados:

| Metodo | Rota | Acesso | Descricao |
|--------|------|--------|-----------|
| POST | `/api/auth/login` | Anonimo | Valida email/senha e retorna JWT |
| POST | `/api/auth/usuarios` | Admin | Cria usuario e atribui uma role |

Ao criar usuario com role `Cliente`, o `ClienteId` e obrigatorio e sua existencia e validada na Application.

O token inclui:

- identificador do usuario;
- email;
- roles;
- claim `cliente_id`, quando aplicavel;
- emissor, audiencia e expiracao.

### Autorizacao

Foi configurada uma fallback policy que exige autenticacao em todos os endpoints, exceto aqueles marcados explicitamente com `AllowAnonymous`.

As permissoes principais ficaram:

- Cliente e Veiculo: `Admin` ou `Atendente`;
- Servico e Peca: `Admin`;
- criar OS: `Admin` ou `Atendente`;
- diagnosticar e finalizar OS: `Admin` ou `Mecanico`;
- entregar e cancelar OS: `Admin` ou `Atendente`;
- gerar e enviar orcamento: `Admin` ou `Atendente`;
- aprovar e rejeitar orcamento: `Cliente` proprietario da OS.

Clientes possuem rotas especificas para consultar a propria OS e o proprio orcamento. A API compara o claim `cliente_id` com o `ClienteId` da Ordem de Servico e responde `403` quando nao existe correspondencia.

### Tratamento global de erros

Foi implementado `GlobalExceptionHandler` com `IExceptionHandler`, mecanismo nativo do ASP.NET Core 8.

O handler converte:

- `ValidationException` em `400` com `ValidationProblemDetails` agrupado por propriedade;
- conflitos de concorrencia e persistencia em `409`;
- `UnauthorizedAccessException` em `401`;
- argumentos invalidos em `400`;
- operacoes invalidas em `409`;
- excecoes inesperadas em `500`, sem expor detalhes internos.

Respostas vazias de `401`, `403` e `404` tambem sao transformadas em ProblemDetails pelo pipeline HTTP.

### Swagger

O Swagger recebeu:

- titulo, versao e descricao da API;
- esquema de seguranca HTTP Bearer;
- campo para informar o JWT;
- aplicacao do token nas chamadas protegidas.

O endpoint de login permanece acessivel sem token para permitir o inicio do fluxo.

---

## Bloco 8 - Testes Unitarios e Cobertura

### Organizacao

O teste placeholder do template foi removido. Os testes reais foram organizados em:

```text
OficinaMecanica.Tests
  Domain/
    Atendimento/
    Estoque/
    Oficina/
    Orcamentacao/
  Application/
    Orcamentacao/
```

### Comportamentos testados

Foram adicionados testes para:

- CPF valido, formatado, repetido e com digito incorreto;
- email valido, normalizacao e formatos invalidos;
- placa antiga, placa Mercosul, normalizacao e formatos invalidos;
- criacao e maquina de estados da Ordem de Servico;
- diagnostico sem itens;
- transicoes invalidas;
- cancelamento e estados terminais;
- calculo do valor total do orcamento;
- envio, aprovacao, rejeicao e cancelamento do orcamento;
- invariantes de `ItemOrcamento`;
- reserva com e sem estoque suficiente;
- emissao de `EstoqueBaixoEvent`;
- consumo e liberacao de reserva;
- tentativa de consumir ou liberar duas vezes;
- entrada e atualizacao de estoque;
- aprovacao transacional com estoque suficiente;
- aprovacao com estoque insuficiente;
- agrupamento de itens repetidos da mesma peca.

Os testes do handler de aprovacao utilizam implementacoes em memoria das interfaces de repositorio e Unit of Work. Eles verificam explicitamente que estoque insuficiente nao altera status, nao cria reserva e nao executa `SaveChangesAsync`.

### Resultado

Foram executados 47 testes:

```text
Aprovados: 47
Falhas: 0
Ignorados: 0
```

Cobertura de linhas das classes criticas:

| Classe | Cobertura de linhas |
|--------|---------------------|
| `AprovarOrcamentoHandler` | 100% |
| `Cpf` | 96,77% |
| `Orcamento` | 91,83% |
| `Placa` | 90,90% |
| `OrdemDeServico` | 87,34% |
| `ItemOrcamento` | 86,95% |
| `Peca` | 86,20% |

A meta de pelo menos 80% foi atingida nas classes de dominio criticas e no caso de uso transacional mais sensivel.

A cobertura global da solucao permanece baixa porque Controllers, repositorios EF Core, Identity e infraestrutura ainda nao possuem testes de integracao. Nao foram criados testes artificiais para elevar esse numero.

### Integracao pendente

Os testes de integracao devem utilizar PostgreSQL real com Testcontainers, conforme a decisao tecnica do projeto. Eles ainda nao foram executados ou adicionados porque o ambiente atual nao possui Docker. Os cenarios prioritarios sao:

- login e autorizacao por roles;
- CRUD de Cliente, Veiculo, Servico e Peca;
- fluxo completo da OS pelo Swagger/API;
- rollback de aprovacao com estoque insuficiente;
- consumo e liberacao de reservas;
- isolamento entre clientes ao consultar OS e orcamentos.

Os diretorios de resultado do Coverlet foram adicionados ao `.gitignore` para impedir o versionamento de artefatos gerados.

---

## Bloco 9 - Indicadores Administrativos

### Tempo medio de execucao

A listagem, o detalhamento e os filtros de OS ja estavam disponiveis, mas o calculo correto do tempo medio exigia registrar o instante em que cada OS entra em execucao.

Foi adicionada a propriedade `DataInicioExecucao` ao aggregate `OrdemDeServico`. Ela e preenchida pelo proprio metodo `IniciarExecucao`, mantendo a transicao e sua data no Domain.

O tempo de execucao de uma OS e calculado por:

```text
DataFinalizacao - DataInicioExecucao
```

Nao e utilizada `DataAbertura`, pois isso incluiria recepcao, diagnostico e espera pela aprovacao do cliente, produzindo uma metrica diferente do tempo de execucao.

Foi adicionada uma Query administrativa que retorna:

- tempo medio em minutos;
- representacao formatada em horas, minutos e segundos;
- valores nulos quando ainda nao existem OS finalizadas.

Endpoint:

| Metodo | Rota | Acesso | Descricao |
|--------|------|--------|-----------|
| GET | `/api/ordens-de-servico/indicadores` | Admin | Retorna indicadores da oficina |

A alteracao de schema e persistida por migration propria.

---

## Bloco 10 - Senha Temporaria e Primeiro Acesso

### Fluxo implementado

A criacao de usuarios deixou de receber uma senha definida pelo administrador. O sistema agora gera uma senha temporaria criptograficamente aleatoria com 16 caracteres e garantia dos grupos exigidos pela politica do Identity:

- letra maiuscula;
- letra minuscula;
- numero;
- caractere especial.

O endpoint administrativo retorna a senha temporaria somente na resposta de criacao:

```json
{
  "usuarioId": "identificador-do-usuario",
  "senhaTemporaria": "valor-gerado"
}
```

A senha nao e persistida em texto puro. O ASP.NET Core Identity armazena apenas o hash.

### Primeiro login

O usuario consegue autenticar com a senha temporaria. O token retornado possui:

```json
{
  "trocaSenhaObrigatoria": true
}
```

e a claim:

```text
troca_senha_obrigatoria = true
```

Enquanto essa claim estiver ativa, o pipeline da API responde `403 Forbidden` para qualquer recurso protegido, exceto a troca de senha.

### Alteracao da senha

Endpoint:

| Metodo | Rota | Acesso | Descricao |
|--------|------|--------|-----------|
| POST | `/api/auth/alterar-senha` | Usuario autenticado | Substitui a senha temporaria |

Corpo:

```json
{
  "senhaAtual": "senha-temporaria",
  "novaSenha": "nova-senha-forte"
}
```

Depois da alteracao, `DeveAlterarSenha` passa para `false`. O usuario deve realizar um novo login para receber um token sem a claim obrigatoria antiga.

### Administrador inicial

O administrador criado pelo seed usa a senha definida externamente em `ADMIN_PASSWORD` e nao e marcado para troca obrigatoria. Isso permite que ele realize o primeiro login e crie os demais usuarios.

### Persistencia

A propriedade `DeveAlterarSenha` foi adicionada ao usuario do Identity e persistida pela migration `AddTrocaSenhaObrigatoria`.

---

## Bloco 11 - Padronizacao das Respostas HTTP

### Problema encontrado

O tratamento de excecoes ja utilizava `ProblemDetails`, mas falhas representadas pelo `Result Pattern` ainda eram convertidas pelos Controllers em objetos anonimos:

```json
{
  "erro": "mensagem"
}
```

Isso produzia dois formatos de erro diferentes na mesma API.

### Alteracao

Foi criada uma extensao compartilhada para Controllers com respostas padronizadas:

- `BusinessRuleProblem`: `400 Bad Request` para regra ou operacao nao atendida;
- `NotFoundProblem`: `404 Not Found` para recurso inexistente.

Todos os Controllers de Cliente, Veiculo, Servico, Peca, Ordem de Servico e Orcamento passaram a utilizar essas respostas.

Formato conceitual:

```json
{
  "title": "Regra de negocio nao atendida",
  "status": 400,
  "detail": "Estoque insuficiente para Filtro.",
  "instance": "/api/orcamentos/00000000-0000-0000-0000-000000000000/aprovar"
}
```

As falhas agora seguem o mesmo padrao utilizado por validacao, autenticacao, autorizacao, conflitos de persistencia e excecoes inesperadas.

### Testes de integracao

A disponibilidade do Docker foi verificada novamente neste bloco. O executavel continua ausente no ambiente, impedindo a criacao e execucao responsavel de testes com Testcontainers e PostgreSQL real.

Os testes de integracao nao foram substituidos por EF Core InMemory ou SQLite, pois esses providers nao reproduzem integralmente transacoes, constraints e comportamento do PostgreSQL usados pelo sistema.

---

## Bloco 12 - Edicao de Itens do Orcamento

O fluxo de geracao continua criando o orcamento a partir dos itens registrados no diagnostico. Foram adicionados casos de uso para complementar manualmente um orcamento antes do envio:

| Metodo | Rota | Acesso | Descricao |
|--------|------|--------|-----------|
| POST | `/api/orcamentos/{orcamentoId}/itens/servicos` | Admin, Atendente | Adiciona um servico do catalogo |
| POST | `/api/orcamentos/{orcamentoId}/itens/pecas` | Admin, Atendente | Adiciona uma peca do estoque |

Os endpoints recebem somente o identificador e a quantidade. Descricao e preco unitario sao obtidos no servidor a partir do cadastro atual, impedindo que o consumidor da API defina valores arbitrarios.

A alteracao e protegida pelo aggregate `Orcamento` e so e permitida no status `Pendente`. Depois que o orcamento e enviado, seus itens e valores ficam congelados. Servicos inativos nao podem ser adicionados.

O `ValorTotal` continua calculado pelo Domain a partir da colecao de itens. Esta mudanca nao exige migration, pois utiliza a colecao de itens ja mapeada.

---

## Bloco 13 - Registro de Servicos Executados

Cada item da Ordem de Servico agora registra individualmente:

- se o servico foi executado;
- data e hora UTC da execucao.

Endpoint:

| Metodo | Rota | Acesso | Descricao |
|--------|------|--------|-----------|
| PATCH | `/api/ordens-de-servico/{ordemDeServicoId}/servicos/{servicoId}/executar` | Admin, Mecanico | Registra a execucao de um item da OS |

O aggregate `OrdemDeServico` valida que:

- a OS esteja em `EmExecucao`;
- o servico exista entre os itens e ainda esteja pendente;
- todos os itens tenham sido executados antes da finalizacao.

Quando existem itens repetidos para o mesmo servico, cada chamada marca o primeiro item ainda pendente. Ao registrar a execucao, o Domain emite `ServicoExecutadoEvent`.

A consulta e a listagem de OS passaram a retornar `Executado` e `DataExecucao` em cada item. A migration `AddControleExecucaoItensOS` adiciona esses campos em `ItensOrdemDeServico`; registros anteriores recebem `Executado = false`.

A finalizacao permanece explicita. O endpoint de finalizar continua responsavel por consumir todas as reservas de pecas e concluir a OS na mesma unidade de trabalho, agora somente depois que todos os servicos forem registrados como executados.

---

## Bloco 14 - Alertas Administrativos de Estoque

O `EstoqueBaixoEvent`, que ja era emitido pelo aggregate `Peca` ao reservar uma quantidade que deixa o estoque disponivel menor ou igual ao minimo, agora possui um consumidor real.

O handler cria um alerta persistente ou atualiza o alerta ativo da mesma peca. Essa deduplicacao impede a criacao de varios alertas abertos para a mesma situacao de estoque.

Cada alerta registra:

- peca e nome da peca;
- quantidade disponivel e quantidade minima no momento do evento;
- data de criacao;
- data de visualizacao;
- data de resolucao.

Endpoints administrativos:

| Metodo | Rota | Acesso | Descricao |
|--------|------|--------|-----------|
| GET | `/api/alertas-estoque?somenteAtivos=true` | Admin | Lista alertas ativos ou todo o historico |
| PATCH | `/api/alertas-estoque/{alertaId}/visualizar` | Admin | Marca o alerta como visualizado |
| PATCH | `/api/alertas-estoque/{alertaId}/resolver` | Admin | Encerra o alerta |

O alerta e uma consequencia administrativa e e processado depois da persistencia da operacao principal. Uma eventual falha na notificacao e registrada no log e nao desfaz uma reserva de estoque valida. Operacoes que exigem consistencia forte, como aprovacao, reserva e mudanca de estado da OS, continuam no fluxo transacional sincrono.

A migration `AddAlertasEstoque` cria a tabela e os indices utilizados pelas consultas.

---

## Bloco 15 - Correcao de Itens do Orcamento

Um orcamento pendente agora pode ter a quantidade de um item corrigida ou um item removido antes do envio.

| Metodo | Rota | Acesso | Descricao |
|--------|------|--------|-----------|
| PUT | `/api/orcamentos/{orcamentoId}/itens/servicos/{servicoId}` | Admin, Atendente | Altera a quantidade do servico |
| DELETE | `/api/orcamentos/{orcamentoId}/itens/servicos/{servicoId}` | Admin, Atendente | Remove o servico |
| PUT | `/api/orcamentos/{orcamentoId}/itens/pecas/{pecaId}` | Admin, Atendente | Altera a quantidade da peca |
| DELETE | `/api/orcamentos/{orcamentoId}/itens/pecas/{pecaId}` | Admin, Atendente | Remove a peca |

O corpo dos endpoints `PUT` recebe apenas `Quantidade`. O preco unitario capturado quando o item foi adicionado e preservado, evitando uma alteracao indireta de preco durante a correcao.

As regras permanecem no aggregate `Orcamento`:

- somente orcamentos `Pendente` podem ser alterados;
- a quantidade deve ser maior que zero;
- o item precisa existir;
- o ultimo item nao pode ser removido, pois um orcamento deve possuir pelo menos um item.

Quando existem itens repetidos com a mesma referencia, a operacao altera ou remove a primeira ocorrencia. Nao houve mudanca de schema neste bloco.

---

## Bloco 16 - Gestao Administrativa de Usuarios

A administracao do ASP.NET Core Identity foi ampliada sem expor `UserManager`, tabelas ou tipos da Infrastructure para a API e Application. Os casos de uso continuam acessando somente `IIdentityService`.

Endpoints:

| Metodo | Rota | Acesso | Descricao |
|--------|------|--------|-----------|
| GET | `/api/auth/usuarios` | Admin | Lista usuarios, roles, vinculo e estado |
| GET | `/api/auth/usuarios/{usuarioId}` | Admin | Consulta um usuario |
| PATCH | `/api/auth/usuarios/{usuarioId}/status` | Admin | Ativa ou desativa o acesso |
| POST | `/api/auth/usuarios/{usuarioId}/redefinir-senha` | Admin | Gera uma nova senha temporaria |

A redefinicao de senha:

- usa o mecanismo de reset do Identity;
- gera uma senha criptograficamente aleatoria;
- retorna a senha somente na resposta;
- volta a marcar a troca de senha como obrigatoria.

O estado atual do usuario e consultado no pipeline em cada requisicao autenticada. Dessa forma, desativacao e redefinicao de senha produzem efeito mesmo sobre JWTs emitidos anteriormente. Usuario desativado recebe `401 Unauthorized`; usuario com troca obrigatoria recebe `403 Forbidden`, exceto na rota de alteracao de senha.

O administrador nao pode desativar o proprio usuario. A criacao tambem passou a rejeitar `ClienteId` para roles internas e continua exigindo um cliente existente para a role `Cliente`.

O bloqueio utiliza os campos de lockout ja existentes no schema do Identity, portanto nao exige nova migration.

---

## Bloco 17 - Alinhamento entre Orcamento e Execucao

Os itens registrados no diagnostico representam a proposta tecnica inicial. Como um orcamento pendente pode receber correcoes antes do envio, esses itens nao podem continuar sendo a fonte definitiva da execucao.

Na aprovacao, a lista de servicos da versao aprovada do orcamento agora substitui os itens de execucao da OS antes da mudanca para `EmExecucao`. Assim:

- servicos removidos do orcamento nao precisam ser executados;
- servicos adicionados ao orcamento passam a exigir registro de execucao;
- quantidades corrigidas sao refletidas na OS;
- a finalizacao verifica exatamente os servicos aceitos pelo cliente.

As pecas continuam controladas diretamente pelos itens do orcamento aprovado e pelas reservas de estoque. Um orcamento nao pode ser enviado nem ter o ultimo servico removido se isso o deixar sem servicos.

Essa preparacao participa do mesmo caso de uso transacional que valida estoque, reserva pecas, aprova o orcamento e inicia a OS. Nao houve mudanca de schema.

---

## Bloco 18 - Listagens da Area do Cliente

O cliente nao precisa mais conhecer previamente o identificador de uma OS ou orcamento para acompanhar seus atendimentos.

Endpoints:

| Metodo | Rota | Acesso | Descricao |
|--------|------|--------|-----------|
| GET | `/api/ordens-de-servico/minhas?status=&pagina=1&tamanhoPagina=20` | Cliente | Lista somente as OS do usuario autenticado |
| GET | `/api/orcamentos/meus?pagina=1&tamanhoPagina=20` | Cliente | Lista somente os orcamentos do usuario autenticado |

O `ClienteId` e obtido exclusivamente da claim `cliente_id`. Ele nao pode ser informado pelo consumidor da API nessas rotas. A consulta de orcamentos correlaciona o orcamento com a OS pertencente ao cliente e possui limite maximo de 100 registros por pagina.

Os endpoints individuais existentes continuam disponiveis e mantem a verificacao de propriedade antes de retornar o recurso.

---

## Bloco 19 - Paginacao das Listagens

As listagens que poderiam carregar tabelas completas passaram a aceitar os parametros padronizados:

```text
pagina = 1
tamanhoPagina = 20
```

O tamanho permitido fica entre 1 e 100 registros. Pagina deve ser maior que zero. Parametros invalidos sao tratados pelo pipeline do FluentValidation antes da execucao do handler.

A paginacao foi aplicada a:

- clientes;
- veiculos por cliente;
- servicos;
- pecas e filtro de estoque baixo;
- usuarios;
- alertas de estoque;
- OS administrativas e do cliente;
- orcamentos do cliente.

As consultas possuem ordenacao deterministica antes de `Skip` e `Take`. Os metodos internos que retornam colecoes completas foram preservados quando necessarios para regras como impedir exclusao de cliente com veiculos ou OS vinculadas. Assim, uma pagina parcial nunca e usada para tomar uma decisao de integridade.

Exemplo:

```http
GET /api/pecas?somenteEstoqueBaixo=true&pagina=1&tamanhoPagina=20
```

Este bloco altera somente queries e nao exige migration.

---

## Bloco 20 - Result Tipado e Status HTTP

O `Result` passou a carregar `ErrorType` alem da mensagem. As categorias disponiveis sao:

- `Validation`;
- `NotFound`;
- `Conflict`;
- `BusinessRule`;
- `Unauthorized`;
- `Forbidden`.

Falhas sem categoria explicita continuam sendo tratadas como regra de negocio, preservando compatibilidade com os metodos de Domain existentes.

A API possui uma unica conversao de `Result` para `ProblemDetails`:

| ErrorType | HTTP | Uso |
|-----------|------|-----|
| Validation | 400 | Dados invalidos |
| BusinessRule | 400 | Transicao ou regra nao atendida |
| Unauthorized | 401 | Credenciais invalidas |
| Forbidden | 403 | Usuario autenticado sem permissao |
| NotFound | 404 | Recurso inexistente |
| Conflict | 409 | Duplicidade, vinculo impeditivo ou conflito de estoque |

Todos os Controllers deixaram de escolher manualmente o status com base apenas na mensagem. Casos de uso classificam a falha e `ControllerExtensions.ToProblem` gera a resposta padronizada.

A categoria tambem e preservada quando um handler converte um `Result<T>` em outro tipo de resultado. Por exemplo, estoque insuficiente permanece `Conflict` ao atravessar o fluxo de aprovacao do orcamento.

Esta alteracao nao modifica persistencia e nao exige migration.

---

## Bloco 21 - Resolucao Automatica de Alertas

Ao adicionar estoque, o caso de uso verifica a quantidade disponivel depois da reposicao. Quando ela fica acima da quantidade minima, o alerta ativo da peca e resolvido automaticamente.

A atualizacao da peca e a resolucao do alerta utilizam o mesmo `SaveChangesAsync`. Se o estoque continuar menor ou igual ao minimo, o alerta permanece aberto. A resolucao manual continua disponivel para situacoes administrativas excepcionais.

---

## Bloco 22 - Integridade Relacional

A migration `AddIntegridadeRelacional` adiciona constraints e indices que impedem registros orfaos mesmo quando dados forem manipulados fora dos Controllers.

Relacionamentos protegidos com `Restrict`:

- Veiculo -> Cliente;
- Ordem de Servico -> Cliente e Veiculo;
- Orcamento -> Ordem de Servico;
- Reserva -> Ordem de Servico;
- Item da OS -> Servico e Peca;
- Item do orcamento -> Servico e Peca;
- Alerta de estoque -> Peca;
- Usuario Cliente -> Cliente.

Indices unicos:

- uma versao por Ordem de Servico em `Orcamentos`;
- um usuario por `ClienteId`, ignorando valores nulos;
- um alerta ativo por peca, mantendo historico de alertas resolvidos.

Os casos de uso foram alinhados às constraints:

- criacao de usuario bloqueia segundo acesso para o mesmo cliente;
- exclusao de cliente bloqueia usuario vinculado;
- exclusao de peca verifica reservas, itens e alertas antes de remover;
- conflitos previsiveis retornam `409` pelo Result tipado.

As configuracoes continuam na Infrastructure por Fluent API. Nenhuma entidade de Domain passou a conhecer Entity Framework ou PostgreSQL.

---

## Bloco 23 - Swagger e Guia Final da API

Foi adicionado um `OperationFilter` para complementar automaticamente cada operacao OpenAPI com:

- resumo baseado no caso de uso;
- roles permitidas ou indicacao de acesso anonimo;
- respostas `ProblemDetails` para 400, 401, 403, 404 e 409 conforme a rota;
- exemplos para login, criacao de usuario, troca de senha e cadastro de cliente;
- ordenacao estavel das operacoes.

O esquema Bearer existente foi preservado. O usuario pode autenticar em `/api/auth/login`, selecionar `Authorize` e informar o token JWT para demonstrar os demais fluxos.

O `README.md` foi reescrito porque seus blocos Markdown estavam corrompidos. O novo guia registra variaveis obrigatorias, execucao Docker/local, primeiro acesso, fluxo completo, testes e links para a documentacao arquitetural.

Com estes blocos, nao restam pendencias funcionais conhecidas no codigo da API da Fase 1. A validacao com PostgreSQL real, Docker Compose e testes de integracao permanece como etapa de ambiente, nao como funcionalidade ausente na API.
