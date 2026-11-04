output "cluster_name" {
  description = "Nome do cluster kind provisionado."
  value       = var.cluster_name
}

output "kubeconfig_path" {
  description = "Caminho do kubeconfig gerado para acessar o cluster."
  value       = pathexpand(var.kubeconfig_path)
}

output "kube_context" {
  description = "Contexto kubectl do cluster kind."
  value       = "kind-${var.cluster_name}"
}

output "namespace" {
  description = "Namespace da aplicacao."
  value       = kubernetes_namespace.oficina.metadata[0].name
}

output "postgres_service" {
  description = "Host do servico de banco de dados dentro do cluster (provisionado pelos manifestos k8s)."
  value       = "postgres.${var.namespace}.svc.cluster.local:5432"
}

output "api_url_nodeport" {
  description = "URL da API exposta via NodePort no host local."
  value       = "http://localhost:30080"
}
