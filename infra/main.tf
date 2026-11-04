# ===========================================================================
# Provisionamento do cluster Kubernetes local (kind) para a Oficina Mecanica.
#
# Recursos criados:
#   - null_resource.kind_cluster  : cria o cluster kind via CLI (versao unica)
#   - kubernetes_namespace.oficina: namespace da aplicacao
#   - kubernetes_secret.oficina   : segredos sensiveis da aplicacao
#   - null_resource.kustomize     : aplica os manifestos k8s (opcional)
#
# Por que criar o cluster pelo CLI e nao pelo provider Terraform de kind?
# Para evitar DUAS versoes de kind no fluxo (a embutida no provider e a do CLI
# usada no "kind load docker-image"). Versoes divergentes geram o erro
# "unknown containerd config version". Usando so o CLI, ha uma unica versao de
# kind/containerd em todo o pipeline.
#
# O banco de dados PostgreSQL NAO e provisionado aqui: ele ja faz parte dos
# manifestos Kubernetes do overlay local (k8s/overlays/local, StatefulSet +
# Service "postgres", imagem oficial postgres:16-alpine). Terraform cuida da
# infra base (cluster + namespace + segredos) e os manifestos sobem app + banco.
#
# Para ambiente cloud (ex.: AWS EKS) troque este null_resource pelo modulo do
# cluster gerenciado e use um banco gerenciado (RDS). Veja infra/README.md.
# ===========================================================================

# ---------------------------------------------------------------------------
# Cria o cluster kind via CLI. O CLI "kind" deve estar no PATH (o pipeline de
# CD instala uma versao fixa; localmente instale conforme infra/README.md).
# A config inline expoe a porta 30080 (NodePort da API) para o host.
# ---------------------------------------------------------------------------
# Config do cluster kind escrita em arquivo para evitar heredoc fragil no shell.
resource "local_file" "kind_config" {
  filename = "${path.module}/.kind-config.yaml"
  content  = <<-EOT
    kind: Cluster
    apiVersion: kind.x-k8s.io/v1alpha4
    nodes:
      - role: control-plane
        extraPortMappings:
          - containerPort: 30080
            hostPort: 30080
            protocol: TCP
      - role: worker
  EOT
}

resource "null_resource" "kind_cluster" {
  triggers = {
    cluster_name = var.cluster_name
    node_image   = var.kind_node_image
    config_hash  = local_file.kind_config.content_md5
  }

  provisioner "local-exec" {
    interpreter = ["/bin/bash", "-c"]
    command     = <<-EOT
      set -euo pipefail
      if ! kind get clusters | grep -qx "${var.cluster_name}"; then
        kind create cluster --name "${var.cluster_name}" --image "${var.kind_node_image}" --wait 120s --config "${local_file.kind_config.filename}"
      fi
    EOT
  }

  provisioner "local-exec" {
    when    = destroy
    command = "kind delete cluster --name ${self.triggers.cluster_name} || true"
  }
}

# O provider kubernetes consome o kubeconfig do contexto gerado pelo kind.
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

  depends_on = [null_resource.kind_cluster]
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
