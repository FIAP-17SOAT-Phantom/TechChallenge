# ===========================================================================
# Provisionamento da infra base no cluster Kubernetes (kind) para a Oficina.
#
# Recursos criados:
#   - kubernetes_namespace.oficina: namespace da aplicacao
#   - kubernetes_secret.oficina   : segredos sensiveis da aplicacao
#   - null_resource.kustomize     : aplica os manifestos k8s (opcional)
#
# IMPORTANTE: o cluster kind NAO e criado por este Terraform. Ele e criado
# ANTES, pelo CLI "kind" (no pipeline de CD ou manualmente, veja infra/README.md).
# Motivos:
#   1. Evita duas versoes de kind no fluxo (provider embutido vs. CLI do
#      "kind load"), que causavam "unknown containerd config version".
#   2. Evita o problema de timing do provider kubernetes, que e configurado no
#      inicio do apply e precisa que o kubeconfig ja exista.
# Assim o Terraform assume que o kubeconfig/contexto "kind-<cluster>" ja existe
# e cuida apenas do namespace + segredos.
#
# O banco de dados PostgreSQL tambem NAO e provisionado aqui: ele faz parte dos
# manifestos Kubernetes do overlay local (k8s/overlays/local, StatefulSet +
# Service "postgres", imagem oficial postgres:16-alpine).
#
# Para ambiente cloud (ex.: AWS EKS) aponte o provider kubernetes para o cluster
# gerenciado e use um banco gerenciado (RDS). Veja infra/README.md.
# ===========================================================================

# O provider kubernetes consome o kubeconfig do contexto gerado pelo kind,
# que ja deve existir (cluster criado antes do terraform apply).
provider "kubernetes" {
  config_path    = pathexpand(var.kubeconfig_path)
  config_context = "kind-${var.cluster_name}"
}

resource "kubernetes_namespace" "oficina" {
  metadata {
    name = var.namespace
    labels = {
      "app.kubernetes.io/part-of" = "oficina-mecanica"
      "managed-by"                = "terraform"
    }
  }
}

# ---------------------------------------------------------------------------
# Segredos da aplicacao (oficina-secrets) consumidos pelos Deployments/Jobs.
# ---------------------------------------------------------------------------
resource "kubernetes_secret" "oficina" {
  metadata {
    name      = "oficina-secrets"
    namespace = kubernetes_namespace.oficina.metadata[0].name
  }

  type = "Opaque"

  data = {
    "postgres-password" = var.postgres_password
    "jwt-secret"        = var.jwt_secret
    "webhook-secret"    = var.webhook_secret
    "admin-email"       = var.admin_email
    "admin-password"    = var.admin_password
    "resend-api-key"    = var.resend_api_key
  }

  depends_on = [kubernetes_namespace.oficina]
}

# ---------------------------------------------------------------------------
# Aplicacao opcional dos manifestos Kustomize apos provisionar a infra.
# Requer kubectl no PATH e o overlay local (k8s/overlays/local).
# ---------------------------------------------------------------------------
resource "null_resource" "kustomize" {
  count = var.apply_k8s_manifests ? 1 : 0

  triggers = {
    cluster = var.cluster_name
  }

  provisioner "local-exec" {
    command = "kubectl --context kind-${var.cluster_name} apply -k ${path.module}/../k8s/overlays/local"
  }

  depends_on = [
    kubernetes_secret.oficina,
  ]
}
