# ===========================================================================
# Provisionamento do cluster Kubernetes local (kind) para a Oficina Mecanica.
#
# Recursos criados:
#   - kind_cluster.oficina        : cluster Kubernetes de 1 control-plane + 1 worker
#   - kubernetes_namespace.oficina: namespace da aplicacao
#   - kubernetes_secret.oficina   : segredos sensiveis da aplicacao
#   - null_resource.kustomize     : aplica os manifestos k8s (opcional)
#
# O banco de dados PostgreSQL NAO e provisionado aqui: ele ja faz parte dos
# manifestos Kubernetes do overlay local (k8s/overlays/local, StatefulSet +
# Service "postgres", imagem oficial postgres:16-alpine). Terraform cuida da
# infra base (cluster + namespace + segredos) e os manifestos sobem app + banco.
#
# Para ambiente cloud (ex.: AWS EKS) substitua o provider "kind" pelo modulo
# do cluster gerenciado e use um banco gerenciado (RDS). Veja infra/README.md.
# ===========================================================================

provider "kind" {}

resource "kind_cluster" "oficina" {
  name            = var.cluster_name
  kubeconfig_path = pathexpand(var.kubeconfig_path)
  wait_for_ready  = true

  kind_config {
    kind        = "Cluster"
    api_version = "kind.x-k8s.io/v1alpha4"

    node {
      role = "control-plane"

      # Expoe a porta do NodePort da API para o host (http://localhost:30080).
      extra_port_mappings {
        container_port = 30080
        host_port      = 30080
        protocol       = "TCP"
      }
    }

    node {
      role = "worker"
    }
  }
}

# O provider kubernetes consome o kubeconfig gerado pelo cluster kind.
provider "kubernetes" {
  host                   = kind_cluster.oficina.endpoint
  client_certificate     = kind_cluster.oficina.client_certificate
  client_key             = kind_cluster.oficina.client_key
  cluster_ca_certificate = kind_cluster.oficina.cluster_ca_certificate
}

resource "kubernetes_namespace" "oficina" {
  metadata {
    name = var.namespace
    labels = {
      "app.kubernetes.io/part-of" = "oficina-mecanica"
      "managed-by"                = "terraform"
    }
  }

  depends_on = [kind_cluster.oficina]
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
    cluster = kind_cluster.oficina.name
  }

  provisioner "local-exec" {
    command = "kubectl --context kind-${var.cluster_name} apply -k ${path.module}/../k8s/overlays/local"
  }

  depends_on = [
    kubernetes_secret.oficina,
  ]
}
