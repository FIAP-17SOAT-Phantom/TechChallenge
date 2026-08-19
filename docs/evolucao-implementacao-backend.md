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
