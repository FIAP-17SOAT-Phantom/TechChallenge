# ===========================================================================
# Provisionamento do cluster Kubernetes local (kind) para a Oficina Mecanica.
#
# Recursos criados:
#   - kind_cluster.oficina        : cluster Kubernetes de 1 control-plane + 1 worker
#   - kubernetes_namespace.oficina: namespace da aplicacao
#   - helm_release.postgres       : banco de dados PostgreSQL (opcional)
#   - kubernetes_secret.oficina   : segredos sensiveis da aplicacao
#   - null_resource.kustomize     : aplica os manifestos k8s (opcional)
#
# Para ambiente cloud (ex.: AWS EKS) substitua o provider "kind" pelo modulo
# do cluster gerenciado e o Helm do PostgreSQL por um banco gerenciado (RDS).
# Veja infra/README.md para o passo a passo.
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

# Os providers kubernetes/helm consomem o kubeconfig gerado pelo cluster kind.
provider "kubernetes" {
  host                   = kind_cluster.oficina.endpoint
  client_certificate     = kind_cluster.oficina.client_certificate
  client_key             = kind_cluster.oficina.client_key
  cluster_ca_certificate = kind_cluster.oficina.cluster_ca_certificate
}

provider "helm" {
  kubernetes {
    host                   = kind_cluster.oficina.endpoint
    client_certificate     = kind_cluster.oficina.client_certificate
    client_key             = kind_cluster.oficina.client_key
    cluster_ca_certificate = kind_cluster.oficina.cluster_ca_certificate
  }
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
# Banco de dados PostgreSQL provisionado no cluster (Helm chart da Bitnami).
# Service resultante: "postgres" no namespace da aplicacao, porta 5432,
# compativel com POSTGRES_HOST=postgres do ConfigMap da aplicacao.
# ---------------------------------------------------------------------------
resource "helm_release" "postgres" {
  count = var.postgres_enabled ? 1 : 0

  name       = "postgres"
  namespace  = kubernetes_namespace.oficina.metadata[0].name
  repository = "https://charts.bitnami.com/bitnami"
  chart      = "postgresql"
  version    = "15.5.38"

  # Mantem o nome do Service como "postgres" (sem sufixo do release).
  set {
    name  = "fullnameOverride"
    value = "postgres"
  }
  set {
    name  = "auth.username"
    value = var.postgres_username
  }
  set {
    name  = "auth.password"
    value = var.postgres_password
  }
  set {
    name  = "auth.database"
    value = var.postgres_database
  }
  set {
    name  = "primary.persistence.size"
    value = var.postgres_storage_size
  }
  set {
    name  = "image.tag"
    value = "16"
  }

  depends_on = [kubernetes_namespace.oficina]
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
    helm_release.postgres,
  ]
}
