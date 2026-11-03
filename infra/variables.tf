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
# Banco de dados (PostgreSQL) provisionado dentro do cluster via Helm/Bitnami.
# ---------------------------------------------------------------------------
variable "postgres_enabled" {
  description = "Provisiona uma instancia PostgreSQL dentro do cluster. Desative para usar um banco gerenciado (RDS, Cloud SQL, etc.)."
  type        = bool
  default     = true
}

variable "postgres_database" {
  description = "Nome do banco de dados da aplicacao."
  type        = string
  default     = "oficina_mecanica"
}

variable "postgres_username" {
  description = "Usuario do banco de dados da aplicacao."
  type        = string
  default     = "oficina"
}

variable "postgres_password" {
  description = "Senha do banco de dados da aplicacao. Nunca versionar: use TF_VAR_postgres_password ou um backend seguro."
  type        = string
  sensitive   = true
}

variable "postgres_storage_size" {
  description = "Tamanho do volume persistente do PostgreSQL."
  type        = string
  default     = "2Gi"
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
