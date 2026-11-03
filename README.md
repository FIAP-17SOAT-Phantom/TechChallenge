# Tech Challenge | Grupo Phantom

## Sistema Integrado de Atendimento e Execucao de Servicos

Backend de gestao de oficina mecanica desenvolvido com .NET 8, Clean Architecture, DDD, CQRS, MediatR, Entity Framework Core e PostgreSQL para o Tech Challenge da FIAP.

---

## Fase 2 - Qualidade, resiliencia e escalabilidade

### Objetivos desta fase

A Fase 2 evolui a aplicacao da Fase 1 para garantir **qualidade, resiliencia e escalabilidade**, incorporando praticas modernas de infraestrutura e automacao:

- **Clean Architecture** com separacao de camadas e dependencias apontando para o dominio;
- **Testes automatizados** (unitarios e de integracao) cobrindo os fluxos criticos da Ordem de Servico;
- **APIs de Ordem de Servico** completas: abertura, consulta de status, aprovacao externa de orcamento, listagem priorizada e notificacao de status por e-mail;
- **Conteinerizacao** com Docker e Docker Compose;
- **Orquestracao** com Kubernetes (Deployments, Services, ConfigMaps, Secrets e HPA);
- **Infraestrutura como Codigo** com Terraform (cluster Kubernetes + banco de dados);
- **CI/CD** com GitHub Actions (build, testes, build/push de imagens, provisionamento e deploy).

### APIs obrigatorias da Fase 2

| Requisito | Endpoint | Observacao |
|-----------|----------|------------|
| Abertura de OS | `POST /api/ordens-de-servico/abertura` | Recebe cliente, veiculo, servicos e pecas; retorna o `id` unico da OS |
| Consulta de status | `GET /api/ordens-de-servico/{id}/status` | Retorna o status atual (Recebida, Em Diagnostico, Aguardando Aprovacao, Em Execucao, Finalizada, Entregue) |
| Aprovacao de orcamento (externa) | `POST /api/webhooks/orcamentos/{id}/resposta` | Webhook idempotente com assinatura HMAC (`X-Webhook-Signature`) para aprovacao/recusa |
| Listagem de OS | `GET /api/ordens-de-servico` | Ordena por prioridade **Em Execucao > Aguardando Aprovacao > Em Diagnostico > Recebida**, mais antigas primeiro, e exclui **logicamente** OS Finalizadas/Entregues/Canceladas |
| Atualizacao de status via e-mail | (automatico) | Cada mudanca de status publica um evento de dominio gravado no outbox transacional; o Worker envia e-mail ao cliente |

> A exclusao das OS Finalizadas/Entregues e **logica** (filtro na consulta), nunca fisica: os registros permanecem no banco e continuam acessiveis por `GET /api/ordens-de-servico/{id}`.

### Arquitetura da solucao

**Componentes da aplicacao**

```text
                       +---------------------------+
   Cliente / Postman   |   API (OficinaMecanica)   |   ASP.NET Core 8
   ----------------->  |  Controllers + MediatR    |   Clean Architecture
                       |  Application -> Domain     |
                       +------------+--------------+
                                    |
                 escreve OutboxMessages (mesma transacao)
                                    v
      +----------------+   +--------+--------+   +------------------------+
      |  PostgreSQL 16 |<--|  Worker (Outbox)|-->|  Provedor de e-mail    |
      |  (EF Core)     |   |  BackgroundSvc   |   |  (Log / Resend)        |
      +----------------+   +-----------------+   +------------------------+
```

- **API**: expoe os endpoints REST, autentica via JWT, aplica regras pela camada de dominio e persiste no PostgreSQL. Eventos de dominio sao gravados na tabela `OutboxMessages` dentro da mesma transacao (outbox transacional).
- **Worker**: processo separado (`OficinaMecanica.Worker`) que faz polling do outbox e envia as notificacoes de e-mail. API e Worker se comunicam apenas pelo banco.
- **PostgreSQL 16**: banco relacional acessado via EF Core (Npgsql).

**Infraestrutura provisionada**

```text
                 Terraform (infra/)
                        |
        +---------------+----------------+
        |                                |
  kind_cluster (K8s)            helm_release (PostgreSQL)
        |                                |
  Namespace oficina-mecanica             |
        |                                |
  +-----+----------------------------+   |
  | Deployment oficina-api (2+ pods) |   |
  | Deployment oficina-worker        |   |
  | Service (NodePort :30080)        |   |
  | ConfigMap + Secret               |<--+ (oficina-secrets via Terraform)
  | HPA (CPU 70% / Mem 80%, 2..6)    |
  | Job de migration do banco        |
  | PodDisruptionBudget              |
  +----------------------------------+
```

**Fluxo de deploy (CI/CD)**

```text
push na branch main
   -> CI (ci.yml): build + testes unitarios + integracao (Testcontainers) + scan
   -> CD (cd.yml):
        1. build + testes (gate)
        2. build e push das imagens Docker (API e Worker) no GHCR
        3. terraform apply  -> provisiona cluster kind + PostgreSQL + Secret
        4. kubectl apply -k -> aplica manifestos, Job de migration e Deployments
        5. rollout status   -> valida o deploy e exibe HPA/pods
```

### Como executar

Tres formas, da mais simples a mais completa:

1. **Local com Docker Compose** - secao [Execucao com Docker](#execucao-com-docker).
2. **Deploy em Kubernetes** - secao [Deploy em Kubernetes](#deploy-em-kubernetes).
3. **Provisionamento com Terraform** - secao [Provisionamento da infraestrutura com Terraform](#provisionamento-da-infraestrutura-com-terraform).

### Entregaveis

- **Colecao das APIs**: Swagger/OpenAPI em `http://localhost:8080/swagger` (JSON em `http://localhost:8080/swagger/v1/swagger.json`). Exemplos de requisicao/resposta em [docs/exemplos-api.md](docs/exemplos-api.md).
- **Video demonstrativo**: _adicionar aqui o link do YouTube/Vimeo (publico ou nao listado, ate 15 min)_.

---

## Funcionalidades

- clientes e veiculos;
- catalogo de servicos;
- pecas, reservas, consumo e alertas de estoque;
- fluxo completo da Ordem de Servico;
- diagnostico e registro individual dos servicos executados;
- orcamento, revisao, aprovacao, rejeicao e nova versao;
- aprovacao e reserva de estoque em transacao serializable;
- indicadores administrativos;
- autenticacao JWT e autorizacao por roles;
- senha temporaria e troca obrigatoria no primeiro acesso;
- area autenticada do cliente;
- respostas de erro com `ProblemDetails`;
- Swagger/OpenAPI com autorizacao Bearer.

## Projetos

| Projeto | Responsabilidade |
|---------|------------------|
| `OficinaMecanica.API` | Controllers, autenticacao, autorizacao, erros e OpenAPI |
| `OficinaMecanica.Application` | Commands, Queries, Handlers, Validators e interfaces |
| `OficinaMecanica.Domain` | Aggregates, entidades, Value Objects, eventos e regras |
| `OficinaMecanica.Infrastructure` | PostgreSQL, EF Core, Identity, repositories e migrations |
| `OficinaMecanica.Tests` | Testes de Domain e Application |
| `OficinaMecanica.IntegrationTests` | Testes HTTP com WebApplicationFactory, Testcontainers e PostgreSQL real |

Direcao das dependencias:

```text
API -> Application -> Domain
Infrastructure -> Application / Domain
```

## Execucao com Docker

Pre-requisitos:

- Docker;
- Docker Compose.

Crie o arquivo `.env` a partir de `.env.example` e defina, no minimo:

```dotenv
JWT_SECRET=uma-chave-com-pelo-menos-32-bytes
ADMIN_PASSWORD=uma-senha-forte
ADMIN_EMAIL=admin@oficina.com
```

Suba API e PostgreSQL:

```bash
docker compose up --build
```

Enderecos:

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- OpenAPI JSON: `http://localhost:8080/swagger/v1/swagger.json`

As migrations sao aplicadas automaticamente na inicializacao. O administrador inicial e criado a partir das variaveis externas.

## Execucao local

O projeto carrega automaticamente o arquivo `.env` da raiz quando iniciado pelo Visual Studio ou por `dotnet run`. Variaveis de ambiente definidas no sistema possuem prioridade e nao sao sobrescritas.

Com PostgreSQL disponivel na connection string configurada:

```powershell
dotnet run --project src/OficinaMecanica.API/OficinaMecanica.API.csproj
```

Sem Docker, instale o PostgreSQL localmente e configure `POSTGRES_DB`, `POSTGRES_USER` e `POSTGRES_PASSWORD` no `.env`, ou forneca `ConnectionStrings__DefaultConnection` diretamente no ambiente. As migrations sao aplicadas automaticamente.

## Deploy em Kubernetes

Os manifestos ficam em `k8s/`, organizados com [Kustomize](https://kustomize.io/):

```text
k8s/
  base/            # Deployment (API), Worker, Service, ConfigMap, Secret (exemplo),
                   # HPA, PodDisruptionBudget, Job de migration, ServiceAccount, Namespace
  overlays/local/  # Cluster local: PostgreSQL (StatefulSet) + imagens locais
```

Pre-requisitos: um cluster Kubernetes (ex.: `kind`, `minikube` ou gerenciado), `kubectl` e `kustomize` (ja embutido no `kubectl`).

1. Crie o Secret real a partir do exemplo (nunca versione o arquivo com valores reais):

   ```bash
   cp k8s/base/secret.example.yaml k8s/base/secret.yaml
   # edite k8s/base/secret.yaml com segredos fortes
   ```

2. Aplique o overlay local (sobe tambem um PostgreSQL no cluster):

   ```bash
   kubectl apply -k k8s/overlays/local
   ```

3. Acompanhe o rollout e verifique o autoscaler:

   ```bash
   kubectl -n oficina-mecanica rollout status deployment/oficina-api
   kubectl -n oficina-mecanica get hpa
   kubectl -n oficina-mecanica get pods
   ```

O **Horizontal Pod Autoscaler** (`k8s/base/hpa.yaml`) escala a API de **2 a 6 replicas** conforme o consumo de CPU (70%) e memoria (80%). Uma **Job de migration** aplica o schema do banco e semeia o administrador antes dos pods da API subirem (init container aguarda as tabelas existirem).

Para simular escalabilidade no video, gere carga nos endpoints e observe o HPA criar replicas:

```bash
kubectl -n oficina-mecanica get hpa -w
```

## Provisionamento da infraestrutura com Terraform

Os scripts Terraform ficam em `infra/` e provisionam **o cluster Kubernetes** (local via `kind`) e **o banco de dados** (PostgreSQL via Helm), alem do Secret da aplicacao.

```bash
cd infra
cp terraform.tfvars.example terraform.tfvars   # preencha os segredos
terraform init
terraform plan
terraform apply
```

Com `apply_k8s_manifests = true`, o Terraform tambem aplica os manifestos Kustomize. A API fica exposta em `http://localhost:30080` (NodePort). Os recursos criados, as variaveis e o passo a passo para nuvem (AWS EKS + RDS) estao documentados em [infra/README.md](infra/README.md).

## Primeiro acesso

1. Autentique o administrador em `POST /api/auth/login`.
2. Use `Authorize` no Swagger informando somente o token, sem escrever `Bearer`.
3. Cadastre Cliente, Veiculo, Servico e Peca.
4. Crie usuarios em `POST /api/auth/usuarios`.
5. Entregue a senha temporaria retornada apenas nessa resposta.
6. O usuario autentica e chama `POST /api/auth/alterar-senha`.
7. Depois da troca, realiza novo login para obter o token definitivo.

## Fluxo principal

```text
Criar Cliente e Veiculo
  -> Criar OS
  -> Iniciar e registrar diagnostico
  -> Gerar e revisar orcamento
  -> Enviar ao cliente
  -> Cliente aprovar
  -> Reservar estoque e iniciar execucao
  -> Registrar servicos executados
  -> Finalizar e consumir reservas
  -> Entregar veiculo
```

## Resumo da API

Todas as rotas, exceto o login, exigem JWT por padrao. As roles indicadas representam qualquer uma das roles autorizadas.

| Metodo | Endpoint | Roles | Finalidade |
|--------|----------|-------|------------|
| `POST` | `/api/auth/login` | Publico | Autenticar e emitir JWT |
| `POST` | `/api/auth/usuarios` | Admin | Criar usuario e senha temporaria |
| `POST` | `/api/auth/alterar-senha` | Autenticado | Trocar senha temporaria ou atual |
| `POST` | `/api/clientes` | Admin, Atendente | Cadastrar cliente |
| `POST` | `/api/veiculos` | Admin, Atendente | Vincular veiculo ao cliente |
| `POST` | `/api/servicos` | Admin | Cadastrar servico |
| `POST` | `/api/pecas` | Admin | Cadastrar peca e estoque inicial |
| `PATCH` | `/api/pecas/{pecaId}/estoque` | Admin | Registrar entrada de estoque |
| `POST` | `/api/ordens-de-servico/abertura` | Admin, Atendente | Abrir OS com cliente, veiculo, servicos e pecas |
| `POST` | `/api/ordens-de-servico` | Admin, Atendente | Abrir OS com cliente e veiculo ja cadastrados (compatibilidade) |
| `PATCH` | `/api/ordens-de-servico/{id}/registrar-diagnostico` | Admin, Mecanico | Registrar diagnostico e itens |
| `POST` | `/api/orcamentos` | Admin, Atendente | Gerar orcamento da OS |
| `PATCH` | `/api/orcamentos/{id}/enviar` | Admin, Atendente | Enviar orcamento ao cliente |
| `PATCH` | `/api/orcamentos/{id}/aprovar` | Cliente | Aprovar e reservar estoque |
| `PATCH` | `/api/orcamentos/{id}/rejeitar` | Cliente | Rejeitar orcamento |
| `PATCH` | `/api/ordens-de-servico/{id}/finalizar` | Admin, Mecanico | Finalizar e consumir reservas |
| `PATCH` | `/api/ordens-de-servico/{id}/entregar` | Admin, Atendente | Registrar entrega do veiculo |
| `GET` | `/api/ordens-de-servico/minhas` | Cliente | Listar as proprias Ordens de Servico |
| `GET` | `/api/ordens-de-servico/indicadores` | Admin | Consultar indicadores administrativos |

A referencia com exemplos completos de login, cadastros, Ordem de Servico, orcamento e respostas de erro esta em [Exemplos da API](docs/exemplos-api.md). O Swagger permanece como fonte completa e executavel das 43 rotas.

### Exemplo rapido de login

```http
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "email": "admin@oficina.local",
  "senha": "senha-configurada-no-env"
}
```

Resposta `200 OK`:

```json
{
  "token": "eyJ...",
  "expiraEm": "2026-09-01T20:00:00Z",
  "email": "admin@oficina.local",
  "roles": ["Admin"],
  "trocaSenhaObrigatoria": false
}
```

## Testes

A solucao possui duas suites:

| Suite | Projeto | Quantidade | Escopo |
|-------|---------|-----------:|--------|
| Unitarios | `OficinaMecanica.Tests` | 141 | Domain, Value Objects, aggregates, validators e Application |
| Integracao | `OficinaMecanica.IntegrationTests` | 9 | HTTP, JWT, MediatR, Identity, EF Core e PostgreSQL 16 real |

Executar os testes unitarios:

```bash
dotnet test src/OficinaMecanica.Tests/OficinaMecanica.Tests.csproj
```

Executar os testes de integracao com Docker Desktop ativo:

```bash
dotnet test src/OficinaMecanica.IntegrationTests/OficinaMecanica.IntegrationTests.csproj
```

Executar toda a solucao:

```bash
dotnet test src/OficinaMecanica.slnx
```

Resultado desta etapa: **121 testes unitarios aprovados e nenhuma falha**. Os 7 testes de integracao da linha de base continuam validados; os 2 novos cenarios de abertura completa estao compilando e aguardam execucao local com o Docker Engine ativo. A camada Domain possui **82,03% de cobertura de linhas** na ultima medicao OpenCover; os percentuais por classe e as instrucoes para gerar o relatorio estao em [Testes e cobertura](docs/relatorio-testes-cobertura.md).

O pipeline de **CI** (`.github/workflows/ci.yml`) executa build Release, testes unitarios com cobertura, testes de integracao com Testcontainers e scan de dependencias a cada push ou pull request para `main`. O pipeline de **CD** (`.github/workflows/cd.yml`) builda e publica as imagens Docker (API e Worker) no GHCR, provisiona o cluster e o banco com Terraform e aplica os manifestos Kubernetes com a Job de migration, validando o rollout.

## Seguranca e qualidade

- ASP.NET Core Identity armazena hashes de senha, nunca senhas em texto puro;
- JWT assinado com HMAC SHA-256, segredo externo e expiracao configuravel;
- autorizacao por roles `Admin`, `Atendente`, `Mecanico` e `Cliente`;
- politica global exige autenticacao, exceto em rotas explicitamente anonimas;
- usuario inativo e bloqueado mesmo que ainda possua token nao expirado;
- senha temporaria aleatoria e troca obrigatoria no primeiro acesso;
- validacao centralizada com FluentValidation;
- erros padronizados com `ProblemDetails`;
- scan NuGet direto e transitivo sem vulnerabilidades conhecidas na ultima validacao;
- cobertura de linhas de 82,03% na camada Domain.

### Resultado do SonarQube Cloud

A analise do commit `12c2716d`, executada em 01/09/2026 pelo GitHub Actions, apresentou Quality Gate aprovado e Security Rating A.

| Metrica | Codigo novo | Codigo geral |
|---------|------------:|-------------:|
| Quality Gate | Aprovado | Aprovado |
| Issues | 0 | 2 historicas |
| Cobertura | 83,33% | 77,8% |
| Duplicacao | 0,0% | 0,0% |
| Security issues | 0 | 0 |
| Security hotspots | 0 | Nao exibido no recorte geral |
| Security Rating | Nao exibido no recorte de codigo novo | A |

A cobertura do codigo novo supera o minimo de 80% configurado no Quality Gate. A verificacao de dependencias NuGet diretas e transitivas tambem nao encontrou vulnerabilidades conhecidas. Na comparacao com a primeira analise, 46 de 48 issues foram encerradas, as 9 security issues foram eliminadas e o Security Rating evoluiu de C para A.

Os detalhes, o escopo das metricas e os achados corrigidos estao no [Relatorio de vulnerabilidades](docs/relatorio-vulnerabilidades.md) e em [Qualidade com Sonar](docs/qualidade-sonar.md). O [dashboard do SonarQube Cloud](https://sonarcloud.io/project/overview?id=FIAP-17SOAT-Phantom_TechChallenge) apresenta a analise vinculada ao repositorio.

## Execucao local e deploy

A aplicacao roda em tres ambientes:

- **Local**: Docker Compose (API + Worker + PostgreSQL) no computador do desenvolvedor;
- **Kubernetes**: manifestos Kustomize em `k8s/`, com HPA, Secrets e Job de migration;
- **Infraestrutura como Codigo**: Terraform em `infra/` provisiona o cluster e o banco.

Em qualquer ambiente, `JWT_SECRET`, credenciais administrativas, segredo do webhook e connection string vem de Secret (Kubernetes/Terraform) ou de `.env` (local), nunca versionados. O banco usa volume persistente (StatefulSet/PVC) ou PostgreSQL gerenciado, com acesso de rede restrito. O pipeline de CI/CD automatiza build, testes, publicacao das imagens e deploy.

## Documentacao

| Documento | Descricao |
|-----------|-----------|
| [Modelagem DDD e Event Storming](docs/plano-bloco1-modelagem-ddd-event-storming.md) | Bounded Contexts, eventos, comandos e aggregates |
| [Decisao arquitetural](docs/decisao-arquitetural-clean-architecture.md) | Clean Architecture |
| [Decisao de banco](docs/decisao-banco-de-dados-postgresql.md) | PostgreSQL |
| [Decisoes tecnicas](docs/decisoes-tecnicas-stack-completa.md) | Stack e justificativas |
| [Roadmap do login e JWT](docs/roadmap-login-autenticacao-jwt.md) | Caminho completo do Controller ao PostgreSQL, emissao do token e autorizacao |
| [Exemplos da API](docs/exemplos-api.md) | Requisicoes e respostas dos principais fluxos |
| [Testes e cobertura](docs/relatorio-testes-cobertura.md) | Organizacao, execucao e cobertura dos dominios criticos |
| [Relatorio de vulnerabilidades](docs/relatorio-vulnerabilidades.md) | Scan de dependencias, achados e correcoes |
| [Qualidade com Sonar](docs/qualidade-sonar.md) | Configuracao, metricas obtidas, linha de base e correcoes do SonarQube Cloud |
| [Infraestrutura (Terraform)](infra/README.md) | Recursos provisionados, variaveis e passo a passo (local e AWS EKS) |

## Grupo

Phantom | FIAP - Pos-graduacao em Software Architecture | Turma 17SOAT

| Membro | RM |
|--------|----|
| Gyovanna de Oliveira Carvalho | 376627 |
| Bruno Russo Ribeiro da Silva | 376557 |

Projeto academico - FIAP 2026.
