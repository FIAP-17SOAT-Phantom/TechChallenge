# Infraestrutura como Codigo (Terraform)

Scripts Terraform que provisionam a **infraestrutura base** do cluster Kubernetes
para a aplicacao da Oficina Mecanica: o cluster, o namespace e os segredos. O
alvo padrao e um cluster local ([kind](https://kind.sigs.k8s.io/)), adequado para
a demonstracao da Fase 2, com instrucoes para adaptar a um cluster gerenciado na
nuvem (AWS EKS).

> O **banco de dados PostgreSQL nao e provisionado pelo Terraform**: ele faz parte
> dos manifestos Kubernetes do overlay local (`k8s/overlays/local`), como um
> StatefulSet + Service `postgres` usando a imagem oficial `postgres:16-alpine`.
> Assim o Terraform cuida da infra base e os manifestos sobem a aplicacao e o
> banco juntos, evitando a dependencia de charts externos instaveis.

## Recursos provisionados

O cluster kind em si e criado pelo CLI `kind` (fora do Terraform) usando
`kind-config.yaml`. O Terraform gerencia os recursos dentro do cluster:

| Recurso Terraform | Tipo | Descricao |
|---|---|---|
| `kubernetes_namespace.oficina` | Namespace | Namespace `oficina-mecanica` onde a aplicacao e implantada. |
| `kubernetes_secret.oficina` | Secret | `oficina-secrets` com senha do banco, JWT, segredo do webhook, admin e Resend. |
| `null_resource.kustomize` | Deploy | Aplica `k8s/overlays/local` via `kubectl apply -k` (opcional, `apply_k8s_manifests`). |

As variaveis e seus valores padrao estao documentados em `variables.tf`.

## Pre-requisitos

- [Terraform](https://developer.hashicorp.com/terraform/downloads) >= 1.6
- [Docker](https://docs.docker.com/get-docker/) em execucao (o kind cria o cluster em containers)
- [kind](https://kind.sigs.k8s.io/docs/user/quick-start/#installation) CLI no PATH. Use a mesma versao do pipeline (v0.27.0).
- [kubectl](https://kubernetes.io/docs/tasks/tools/) no PATH

## Como aplicar

O cluster kind e criado pelo CLI **antes** do Terraform (que cuida so de
namespace + segredos). Isso evita problemas de versao de kind e de timing do
provider kubernetes.

```bash
cd infra

# 1. Preencha os segredos
cp terraform.tfvars.example terraform.tfvars
#   edite terraform.tfvars com senhas reais

# 2. Crie o cluster kind (uma unica versao de kind em todo o fluxo)
kind create cluster --name oficina-mecanica \
  --image kindest/node:v1.32.2 --wait 120s --config kind-config.yaml

# 3. Inicialize os providers e provisione namespace + segredos
terraform init
terraform plan
terraform apply

# 4. (opcional) ver saidas uteis
terraform output
```

Com `apply_k8s_manifests = true`, o `terraform apply` ja aplica os manifestos
Kustomize (aplicacao + banco). Caso contrario, aplique manualmente:

```bash
kubectl --context kind-oficina-mecanica apply -k ../k8s/overlays/local
```

A API fica acessivel em `http://localhost:30080` (NodePort mapeado pelo kind).

## Destruir

```bash
# Remove namespace + segredos gerenciados pelo Terraform
terraform destroy

# Remove o cluster kind (e tudo que roda nele, incluindo o banco e seus volumes)
kind delete cluster --name oficina-mecanica
```

## Usando um banco de dados gerenciado

Para apontar a aplicacao a um banco externo (ex.: AWS RDS, Azure Database,
Cloud SQL) em vez do PostgreSQL do overlay local:

1. Remova (ou nao aplique) o `postgres-statefulset.yaml`/`postgres-service.yaml`
   do overlay `k8s/overlays/local`.
2. Ajuste `POSTGRES_HOST` no `k8s/base/configmap.yaml` (ou em um overlay) para o
   endpoint do banco gerenciado.
3. Garanta que `postgres_password` (Secret `oficina-secrets`) corresponde a senha
   do banco gerenciado.

## Adaptando para AWS EKS (nuvem)

Para provisionar na nuvem em vez do kind:

1. Em vez de criar o cluster com o CLI `kind`, use o modulo oficial
   [`terraform-aws-modules/eks/aws`](https://registry.terraform.io/modules/terraform-aws-modules/eks/aws/latest)
   (cria VPC, control-plane EKS e node groups).
2. Provisione o banco com
   [`terraform-aws-modules/rds/aws`](https://registry.terraform.io/modules/terraform-aws-modules/rds/aws/latest)
   e ajuste `POSTGRES_HOST` no ConfigMap para o endpoint do RDS.
3. Aponte o provider `kubernetes` para o endpoint e o token do EKS
   (via `data.aws_eks_cluster` / `data.aws_eks_cluster_auth`).

O restante (namespace, Secret, aplicacao dos manifestos) permanece igual, pois
os manifestos Kustomize sao agnosticos ao provedor do cluster.
