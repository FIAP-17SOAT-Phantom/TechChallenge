terraform {
  required_version = ">= 1.6.0"

  required_providers {
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.33"
    }
    null = {
      source  = "hashicorp/null"
      version = "~> 3.2"
    }
  }
}
