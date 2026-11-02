# Plano de Execucao — Tech Challenge Fase 2

## 1. Objetivo da fase

Evoluir o backend entregue na Fase 1 para suportar maior demanda, alta disponibilidade, provisionamento automatizado e deploy em Kubernetes, mantendo Clean Architecture, qualidade, testes e segurança.

Este documento compara o PDF oficial da Fase 2 com o estado atual do repositorio em 02/11/2026.

## 2. Linha de base atual

O projeto ja possui:

- monolito modular em .NET 8;
- Clean Architecture, DDD, CQRS e MediatR;
- PostgreSQL com Entity Framework Core e migrations;
- autenticacao JWT e autorizacao por roles;
- Dockerfile multi-stage executando com usuario nao root;
- Docker Compose para API e PostgreSQL;
- Swagger/OpenAPI;
- pipeline de build, testes, cobertura, scan de pacotes e SonarQube Cloud;
- 96 testes unitarios e 7 testes de integracao registrados na Fase 1;
- Quality Gate aprovado e Security Rating A na ultima analise documentada.

Validacao da linha de base:

- build da solution: sucesso, zero erros e zero avisos;
- testes unitarios: 96 aprovados;
- diretorio `/k8s`: inexistente;
- diretorio `/infra`: inexistente;
- collection Postman ou OpenAPI versionada: inexistente.

## 3. Matriz de aderencia

| Requisito da Fase 2 | Estado atual | Acao necessaria |
|---|---|---|
| Clean Code e arquitetura em camadas | Atendido | Manter e revisar somente os fluxos alterados |
| Testes dos fluxos criticos | Parcial | Cobrir as novas APIs, notificacoes e regras de listagem |
| Abertura da OS com cliente, veiculo, servicos e pecas | Atendido | Implementado em `POST /api/ordens-de-servico/abertura` com transacao unica |
| Consulta do status da OS | Parcial | O detalhe atual retorna status; criar resposta especifica e documentada |
| Aprovacao ou recusa por notificacao externa | Nao atendido | Criar webhook seguro e idempotente |
| Lista operacional ordenada por prioridade e antiguidade | Nao atendido | Criar consulta especifica no repositorio |
| Excluir finalizadas e entregues da lista operacional | Nao atendido | Filtrar logicamente sem remover registros |
| Atualizacao de status por e-mail | Nao atendido | Implementar notificacao assíncrona e confiavel |
| Dockerfile atualizado | Atendido parcialmente | Otimizar build e adicionar health check |
| Docker Compose local | Atendido parcialmente | Adicionar health check da API e configuracoes da notificacao |
| Kubernetes Deployment e Service | Nao atendido | Criar manifestos em `/k8s` |
| ConfigMap e Secret | Nao atendido | Criar templates sem segredos reais |
| HPA por CPU e memoria | Nao atendido | Criar HPA v2, requests e limits |
| Terraform para cluster e banco | Nao atendido | Criar scripts em `/infra` |
| CI/CD com imagem, banco e Kubernetes | Parcial | Ampliar GitHub Actions para CD completo |
| README da Fase 2 e arquitetura | Nao atendido | Atualizar apos a implementacao |
| Collection completa da API | Nao atendido | Versionar OpenAPI ou Postman collection |
| Video de ate 15 minutos | Externo | Gravar apos ambiente e HPA funcionarem |
| PDF final | Externo | Incluir repositorio, arquitetura e video |

## 4. Alteracoes na aplicacao

### 4.1 Abertura completa da Ordem de Servico

**Estado: concluido em 02/11/2026.**

Foi criado `POST /api/ordens-de-servico/abertura`, preservando o endpoint anterior para compatibilidade. O novo contrato recebe cliente, veiculo, listas independentes de servicos e pecas e retorna o ID e o numero da OS. O caso de uso localiza ou cria cliente pelo CPF, localiza ou cria veiculo pela placa, rejeita placa pertencente a outro cliente, valida servicos ativos e pecas existentes em duas consultas em lote e persiste tudo em uma unica transacao e um unico `SaveChangesAsync`. Dados divergentes de um CPF ou placa existente retornam `409`, evitando sobrescrever ou ignorar silenciosamente o cadastro mestre.

As colecoes `ServicosSolicitados` e `PecasSolicitadas` foram separadas dos itens definidos no diagnostico. Assim, `RegistrarDiagnostico` pode substituir os itens de execucao sem apagar a solicitacao original do cliente. A abertura nao reserva estoque; essa responsabilidade continua no fluxo de aprovacao do orcamento.

A migration `AddAberturaCompletaOrdemDeServico` cria as tabelas das solicitacoes, constraints de quantidade, indices unicos por OS e relacionamentos restritivos com Servico e Peca. Uma chave de idempotencia para repeticoes HTTP continua como melhoria opcional; ela nao faz parte do requisito funcional explicito da abertura e sera reavaliada junto ao webhook idempotente.

### 4.2 Geracao concorrente do numero da OS

**Estado: concluido em 02/11/2026.**

`COUNT + 1` foi substituido pela sequence PostgreSQL `ordem_servico_numero_seq`. A migration inicializa a sequence depois do maior numero ja gravado, preservando bancos da Fase 1. Como `nextval` e atomico, replicas simultaneas nao recebem o mesmo numero; o indice unico em `Numero` permanece como protecao adicional. Saltos na numeracao podem ocorrer quando uma transacao falha, comportamento esperado de sequences. O teste concorrente com PostgreSQL permanece na lista de validacoes de integracao da fase.

### 4.3 Consulta de status

O endpoint de detalhe ja retorna o status, mas a Fase 2 pede explicitamente uma consulta de situacao.

Adicionar uma resposta enxuta, por exemplo:

```text
GET /api/ordens-de-servico/{id}/status
```

Campos sugeridos:

- ID e numero da OS;
- status atual;
- data da ultima atualizacao;
- datas de abertura, inicio e finalizacao quando aplicaveis.

Para isso, adicionar `DataUltimaAtualizacao` ao agregado ou manter historico de transicoes.

### 4.4 Aprovacao externa do orcamento

Os endpoints atuais exigem JWT de Cliente e representam uma acao direta. A Fase 2 exige recebimento de notificacao externa.

Criar um webhook separado, mantendo os endpoints atuais:

```text
POST /api/webhooks/orcamentos/{id}/resposta
```

Requisitos do webhook:

- aceitar aprovacao ou recusa;
- validar assinatura HMAC ou API key armazenada em Secret;
- exigir `externalEventId` unico;
- ser idempotente;
- rejeitar timestamp expirado e replay;
- registrar recebimento, resultado e data;
- reutilizar os Commands existentes de aprovacao e rejeicao;
- retornar sucesso para repeticoes ja processadas sem duplicar efeitos.

### 4.5 Fila operacional de Ordens de Servico

Criar uma query especifica para a listagem operacional. A ordenacao obrigatoria e:

1. `EmExecucao`;
2. `AguardandoAprovacao`;
3. `EmDiagnostico`;
4. `Recebida`;
5. dentro do mesmo status, mais antigas primeiro.

O repositorio atual ordena apenas por `DataAbertura` decrescente e inclui estados encerrados.

Implementar a prioridade no SQL/EF Core antes de `Skip` e `Take`. A lista operacional deve excluir `Finalizada` e `Entregue` sem excluir fisicamente os registros. Recomenda-se excluir tambem `Cancelada`, pois ela nao aparece na fila de trabalho, mantendo um filtro separado para historico.

### 4.6 Notificacoes de status por e-mail

Criar uma abstracao na Application, por exemplo `INotificacaoClienteService`, e uma implementacao na Infrastructure para o provedor escolhido.

Notificar, no minimo:

- OS recebida;
- diagnostico iniciado;
- aguardando aprovacao;
- orcamento enviado;
- orcamento aprovado ou recusado;
- execucao iniciada;
- OS finalizada;
- veiculo entregue;
- OS cancelada.

Para resiliencia, nao enviar e-mail dentro da transacao HTTP. Implementar:

- evento de alteracao de status;
- tabela Outbox;
- worker em segundo plano;
- retry com backoff;
- registro de tentativa, erro e conclusao;
- idempotencia por mensagem.

As credenciais do provedor devem vir de variavel de ambiente e Kubernetes Secret.

### 4.7 Health checks e execucao com multiplas replicas

Adicionar:

- `/health/live` para liveness;
- `/health/ready` verificando PostgreSQL e dependencias obrigatorias;
- encerramento gracioso;
- timeouts e resiliencia no cliente do provedor de e-mail.

Hoje as migrations e o seed executam durante o startup da API. Com varias replicas isso pode provocar concorrencia. Mover migrations e seed para um Job Kubernetes ou etapa unica do pipeline antes do rollout.

## 5. Testes a adicionar

### Unitarios

- abertura completa com cliente e veiculo existentes;
- abertura com cadastro de novos dados;
- veiculo pertencente a outro cliente;
- servico inativo ou inexistente;
- peca inexistente e quantidade invalida;
- geracao concorrente de numero da OS;
- ordenacao por prioridade;
- mais antigas primeiro dentro do mesmo status;
- exclusao de finalizadas, entregues e canceladas da fila;
- webhook aprovado, recusado, repetido e com assinatura invalida;
- criacao de mensagens Outbox para cada transicao relevante.

### Integracao

- fluxo completo de abertura da OS em uma unica requisicao;
- consulta de status;
- listagem operacional paginada e ordenada;
- webhook real contra PostgreSQL;
- idempotencia concorrente;
- processamento e retry da Outbox com provedor fake;
- migrations executadas fora do startup da API;
- endpoints de health check.

### Infraestrutura e carga

- smoke test apos deploy;
- teste com `k6` ou ferramenta equivalente para gerar carga;
- evidencia de aumento e reducao de replicas pelo HPA;
- teste de indisponibilidade temporaria do provedor de e-mail;
- teste de restart de pod sem perda dos dados.

## 6. Docker

O Dockerfile atual ja e multi-stage e executa como usuario nao root. Ajustes recomendados:

- remover o `dotnet build` redundante antes de `dotnet publish` ou usar `--no-restore` corretamente;
- criar `src/.dockerignore` para excluir `bin`, `obj`, testes e artefatos;
- expor e utilizar health check da API;
- revisar tags e versoes das imagens;
- adicionar health check da API no Compose;
- adicionar variaveis do provedor de notificacao;
- manter segredos somente no `.env` local.

## 7. Kubernetes — diretorio `/k8s`

Criar, no minimo:

```text
k8s/
  base/
    namespace.yaml
    deployment.yaml
    service.yaml
    configmap.yaml
    secret.example.yaml
    hpa.yaml
    migration-job.yaml
    service-account.yaml
    kustomization.yaml
  overlays/
    local/
    production/
```

O `Deployment` deve conter:

- imagem versionada por SHA ou tag imutavel;
- pelo menos duas replicas no ambiente demonstrado;
- liveness e readiness probes;
- requests e limits de CPU e memoria;
- variaveis vindas de ConfigMap e Secret;
- estrategia RollingUpdate;
- configuracao de encerramento gracioso.

O `Service` deve expor a API dentro do cluster. Para acesso externo, adicionar Ingress ou Service do tipo LoadBalancer conforme o ambiente escolhido.

O `HPA` deve usar `autoscaling/v2` e metricas de CPU e memoria. O cluster precisa de Metrics Server. Requests de recursos sao obrigatorios para o calculo do HPA.

Caso o PostgreSQL seja executado no cluster local, adicionar StatefulSet ou Helm chart, Service, Secret e PVC. Em cloud, apontar a API para o banco gerenciado provisionado pelo Terraform.

## 8. Terraform — diretorio `/infra`

Antes de implementar, escolher um alvo:

### Opcao A — cloud

Exemplo AWS:

- VPC, subnets e security groups;
- EKS e node group;
- RDS PostgreSQL;
- ECR para imagens;
- IAM e OIDC para GitHub Actions;
- Secrets Manager ou integracao equivalente;
- outputs de endpoint, registry e cluster.

Vantagem: demonstra deploy real e alta disponibilidade. Desvantagem: custo, tempo de provisionamento e necessidade de destruir recursos apos a gravacao.

### Opcao B — cluster local/efemero

- kind, k3d ou Minikube;
- PostgreSQL em StatefulSet/Helm com PVC;
- Terraform ou provider apropriado para criar cluster e recursos;
- pipeline com cluster efemero no runner ou self-hosted runner.

Vantagem: sem custo. Desvantagem: ambiente nao fica publicamente disponivel e o GitHub-hosted runner nao alcanca o cluster da maquina local.

Estrutura sugerida:

```text
infra/
  versions.tf
  providers.tf
  variables.tf
  main.tf
  outputs.tf
  terraform.tfvars.example
  modules/
  README.md
```

Nunca versionar `terraform.tfstate`, `.terraform`, arquivos reais de variaveis ou credenciais. Configurar backend remoto e lock de state quando o alvo for cloud.

## 9. CI/CD

Os workflows atuais cobrem build, testes, vulnerabilidades e SonarQube, mas nao realizam entrega continua.

Separar ou organizar os pipelines em:

### CI — pull request e push

1. restore;
2. build Release;
3. testes unitarios e integracao;
4. cobertura;
5. scan de dependencias;
6. SonarQube;
7. build da imagem Docker;
8. scan da imagem;
9. validacao de Terraform e manifestos Kubernetes.

### CD — main ou acionamento manual

1. autenticar no provedor por OIDC, sem chave permanente;
2. executar `terraform fmt`, `validate`, `plan` e `apply`;
3. construir e publicar imagem no registry com tag do commit;
4. atualizar a referencia da imagem;
5. aplicar ConfigMap, Service, Deployment e HPA;
6. executar Job de migration/seed uma unica vez;
7. aguardar rollout;
8. executar smoke tests;
9. disponibilizar evidencias no summary do workflow.

Usar GitHub Environments e aprovacao manual para producao. Os segredos necessarios incluem credenciais/OIDC do provedor, token do servico de e-mail, segredo JWT e dados administrativos iniciais.

## 10. Documentacao e entregaveis

Atualizar o README com:

- objetivo e escopo da Fase 2;
- desenho dos componentes da aplicacao;
- desenho da infraestrutura provisionada;
- fluxo de CI/CD;
- execucao local com Docker Compose;
- provisionamento com Terraform;
- deploy e operacao no Kubernetes;
- comandos para validar HPA;
- estrategia de migrations e secrets;
- link para a collection completa da API;
- link do video final.

Criar tambem:

- diagrama de arquitetura da aplicacao;
- diagrama da infraestrutura;
- diagrama do fluxo de deploy;
- OpenAPI exportado ou collection Postman versionada;
- roteiro de video mostrando deploy, CI/CD, APIs e HPA;
- PDF final com repositorio, arquitetura e video;
- acesso ao repositorio privado para `soat-architecture`.

## 11. Ordem recomendada de implementacao

1. escolher Kubernetes local ou cloud e o provedor de e-mail;
2. criar branch da Fase 2 e preservar a linha de base;
3. modelar a abertura completa da OS e historico de itens;
4. corrigir a geracao concorrente do numero da OS;
5. implementar status e fila operacional ordenada;
6. implementar webhook externo seguro e idempotente;
7. implementar eventos de status, Outbox e e-mail;
8. adicionar health checks e retirar migrations do startup multi-replica;
9. ampliar testes unitarios e de integracao;
10. revisar Dockerfile, `.dockerignore` e Compose;
11. criar manifestos Kubernetes e validar localmente;
12. criar Terraform para cluster e banco;
13. ampliar CI/CD ate o deploy e smoke test;
14. executar teste de carga e demonstrar HPA;
15. atualizar README, collection, diagramas e evidencias;
16. gravar o video e montar o PDF final.

## 12. Criterios de pronto

- abertura da OS recebe todos os dados obrigatorios e e transacional;
- status pode ser consultado por endpoint documentado;
- webhook externo aprova ou recusa sem duplicar efeitos;
- fila operacional possui prioridade e antiguidade corretas;
- encerradas nao aparecem na fila e permanecem no historico;
- cliente recebe notificacoes de status com retry confiavel;
- API executa com varias replicas sem colisao de numero, migrations ou seed;
- Docker Compose continua funcionando localmente;
- Kubernetes possui Deployment, Service, ConfigMap, Secret, HPA e probes;
- Terraform cria cluster e banco de forma reproduzivel;
- pipeline executa build, testes, imagem, banco e deploy;
- HPA e demonstrado sob carga;
- documentacao, collection, video e PDF estao acessiveis.
