# Infraestrutura como Codigo (Terraform)

Scripts Terraform que provisionam **o cluster Kubernetes** e **o banco de dados**
para a aplicacao da Oficina Mecanica. O alvo padrao e um cluster local
([kind](https://kind.sigs.k8s.io/)), adequado para a demonstracao da Fase 2, com
instrucoes para adaptar a um cluster gerenciado na nuvem (AWS EKS).

## Recursos provisionados

| Recurso Terraform | Tipo | Descricao |
|---|---|---|
| `kind_cluster.oficina` | Cluster Kubernetes | Cluster local com 1 control-plane + 1 worker. Mapeia a porta `30080` do NodePort para o host. |
| `kubernetes_namespace.oficina` | Namespace | Namespace `oficina-mecanica` onde a aplicacao e implantada. |
| `helm_release.postgres` | Banco de dados | PostgreSQL 16 (chart Bitnami) exposto como Service `postgres:5432`. Opcional (`postgres_enabled`). |
| `kubernetes_secret.oficina` | Secret | `oficina-secrets` com senha do banco, JWT, segredo do webhook, admin e Resend. |
| `null_resource.kustomize` | Deploy | Aplica `k8s/overlays/local` via `kubectl apply -k` (opcional, `apply_k8s_manifests`). |

As variaveis e seus valores padrao estao documentados em `variables.tf`.

## Pre-requisitos

- [Terraform](https://developer.hashicorp.com/terraform/downloads) >= 1.6
- [Docker](https://docs.docker.com/get-docker/) em execucao (o kind cria o cluster em containers)
- [kubectl](https://kubernetes.io/docs/tasks/tools/) (necessario se `apply_k8s_manifests = true`)

## Como aplicar

```bash
cd infra

# 1. Preencha os segredos
cp terraform.tfvars.example terraform.tfvars
#   edite terraform.tfvars com senhas reais

# 2. Inicialize os providers
terraform init

# 3. Revise o plano
terraform plan

# 4. Provisione cluster + banco + segredos
terraform apply

# 5. (opcional) ver saidas uteis
terraform output
```

Com `apply_k8s_manifests = true`, o `terraform apply` ja aplica os manifestos
Kustomize. Caso contrario, aplique manualmente:

```bash
kubectl --context kind-oficina-mecanica apply -k ../k8s/overlays/local
```

A API fica acessivel em `http://localhost:30080` (NodePort mapeado pelo kind).

## Destruir

```bash
terraform destroy
```

Isso remove o cluster kind (e tudo que roda nele). Volumes do PostgreSQL vivem
dentro do cluster e sao descartados junto.

## Usando um banco de dados gerenciado

Para apontar a aplicacao a um banco externo (ex.: AWS RDS, Azure Database,
Cloud SQL):

1. Defina `postgres_enabled = false`.
2. Ajuste `POSTGRES_HOST` no `k8s/base/configmap.yaml` (ou em um overlay) para o
   endpoint do banco gerenciado.
3. Garanta que `postgres_password` corresponde a senha do banco gerenciado.

## Adaptando para AWS EKS (nuvem)

Para provisionar na nuvem em vez do kind:

1. Substitua o provider `kind` e o recurso `kind_cluster` pelo modulo oficial
   [`terraform-aws-modules/eks/aws`](https://registry.terraform.io/modules/terraform-aws-modules/eks/aws/latest)
   (cria VPC, control-plane EKS e node groups).
2. Troque o `helm_release.postgres` por
   [`terraform-aws-modules/rds/aws`](https://registry.terraform.io/modules/terraform-aws-modules/rds/aws/latest)
   e defina `postgres_enabled = false`.
3. Aponte os providers `kubernetes`/`helm` para o endpoint e o token do EKS
   (via `data.aws_eks_cluster` / `data.aws_eks_cluster_auth`).

O restante (namespace, Secret, aplicacao dos manifestos) permanece igual, pois
os manifestos Kustomize sao agnosticos ao provedor do cluster.
