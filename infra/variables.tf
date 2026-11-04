variable "cluster_name" {
  description = "Nome do cluster Kubernetes local (kind) a ser provisionado."
  type        = string
  default     = "oficina-mecanica"
}

variable "namespace" {
  description = "Namespace onde a aplicacao da oficina sera implantada."
  type        = string
  default     = "oficina-mecanica"
}

variable "kubeconfig_path" {
  description = "Caminho onde o kubeconfig do cluster kind sera gravado."
  type        = string
  default     = "~/.kube/config"
}

# ---------------------------------------------------------------------------
# Banco de dados. O PostgreSQL e provisionado pelos manifestos Kubernetes
# (k8s/overlays/local), nao pelo Terraform. A senha abaixo so alimenta o
# Secret "oficina-secrets" (chave postgres-password) consumido pelo banco e
# pela aplicacao. Para um banco gerenciado (RDS/Cloud SQL), aponte o
# POSTGRES_HOST no ConfigMap e use esta senha.
# ---------------------------------------------------------------------------
variable "postgres_password" {
  description = "Senha do banco de dados da aplicacao. Nunca versionar: use TF_VAR_postgres_password ou um backend seguro."
  type        = string
  sensitive   = true
}

# ---------------------------------------------------------------------------
# Segredos da aplicacao criados como Secret do Kubernetes (oficina-secrets).
# ---------------------------------------------------------------------------
variable "jwt_secret" {
  description = "Segredo usado para assinar tokens JWT (minimo 32 bytes)."
  type        = string
  sensitive   = true
}

variable "webhook_secret" {
  description = "Segredo HMAC para validar webhooks de aprovacao de orcamento (minimo 32 bytes)."
  type        = string
  sensitive   = true
}

variable "admin_email" {
  description = "E-mail do usuario administrador semeado na primeira execucao."
  type        = string
  default     = "admin@oficina.local"
}

variable "admin_password" {
  description = "Senha do usuario administrador semeado na primeira execucao."
  type        = string
  sensitive   = true
}

variable "resend_api_key" {
  description = "Chave de API do provedor de e-mail Resend. Opcional (apenas se Email__Provider = Resend)."
  type        = string
  sensitive   = true
  default     = ""
}

variable "apply_k8s_manifests" {
  description = "Aplica automaticamente os manifestos Kustomize (k8s/overlays/local) apos provisionar o cluster."
  type        = bool
  default     = false
}
