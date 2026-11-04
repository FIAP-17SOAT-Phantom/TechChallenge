# ===========================================================================
# demo-k8s.ps1 - Sobe a aplicacao no Kubernetes (kind) via Terraform, LOCALMENTE.
#
# Reproduz, na sua maquina, o mesmo fluxo do pipeline de CD (.github/workflows/cd.yml):
#   1. Cria o cluster Kubernetes (kind)
#   2. Instala o metrics-server (para o HPA)
#   3. Builda as imagens Docker (API + Worker) localmente
#   4. Carrega as imagens no cluster kind
#   5. Provisiona namespace + segredos com Terraform
#   6. Aplica os manifestos k8s (postgres, migration, API, worker, HPA)
#   7. Aguarda tudo ficar pronto e mostra como acessar o Swagger
#
# Uso (PowerShell, na raiz do projeto):
#   ./demo-k8s.ps1
#
# Para destruir tudo no fim da gravacao:
#   ./demo-k8s.ps1 -Destroy
# ===========================================================================
param(
    [switch]$Destroy
)

# NAO usamos "Stop" global: ferramentas nativas (kind/kubectl/docker/terraform)
# escrevem progresso no stderr mesmo em caso de sucesso, o que derrubaria o
# script. Em vez disso checamos $LASTEXITCODE apos cada passo critico (Assert).
$ErrorActionPreference = "Continue"

$ClusterName = "oficina-mecanica"
$Ctx         = "kind-$ClusterName"
$Ns          = "oficina-mecanica"
$ImageApi    = "techchallenge-api"
$ImageWorker = "techchallenge-worker"
$Tag         = "local"

# Garante que kind/terraform instalados via winget estejam no PATH desta sessao.
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + `
            [System.Environment]::GetEnvironmentVariable("Path","User")

function Section($msg) {
    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host ">>> $msg" -ForegroundColor Cyan
    Write-Host "============================================================" -ForegroundColor Cyan
}

function Assert($msg) {
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERRO: $msg (exit code $LASTEXITCODE)" -ForegroundColor Red
        exit 1
    }
}

if ($Destroy) {
    Section "Destruindo o cluster kind '$ClusterName'"
    kind delete cluster --name $ClusterName
    Write-Host "Cluster removido. Fim." -ForegroundColor Green
    return
}

# ---------------------------------------------------------------------------
Section "1/7 Criando o cluster Kubernetes (kind)"
$existing = (kind get clusters 2>$null) -split "`n"
if ($existing -contains $ClusterName) {
    Write-Host "Cluster '$ClusterName' ja existe, reutilizando." -ForegroundColor Yellow
} else {
    kind create cluster --name $ClusterName --image kindest/node:v1.32.2 --wait 120s --config infra/kind-config.yaml
    Assert "falha ao criar o cluster kind"
    kubectl --context $Ctx wait --for=condition=Ready nodes --all --timeout=180s
    Assert "nodes do cluster nao ficaram Ready"
}

# ---------------------------------------------------------------------------
Section "2/7 Instalando metrics-server (para o HPA)"
kubectl --context $Ctx apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
Assert "falha ao aplicar o metrics-server"
kubectl --context $Ctx -n kube-system patch deployment metrics-server --type=json `
    -p='[{\"op\":\"add\",\"path\":\"/spec/template/spec/containers/0/args/-\",\"value\":\"--kubelet-insecure-tls\"}]'
kubectl --context $Ctx -n kube-system rollout status deployment/metrics-server --timeout=120s

# ---------------------------------------------------------------------------
Section "3/7 Buildando as imagens Docker (API + Worker)"
docker build -t "${ImageApi}:${Tag}"    -f src/OficinaMecanica.API/Dockerfile    src
Assert "falha no build da imagem da API"
docker build -t "${ImageWorker}:${Tag}" -f src/OficinaMecanica.Worker/Dockerfile src
Assert "falha no build da imagem do Worker"

# ---------------------------------------------------------------------------
Section "4/7 Carregando as imagens no cluster kind"
kind load docker-image "${ImageApi}:${Tag}"    --name $ClusterName
Assert "falha ao carregar a imagem da API no kind"
kind load docker-image "${ImageWorker}:${Tag}" --name $ClusterName
Assert "falha ao carregar a imagem do Worker no kind"

# ---------------------------------------------------------------------------
Section "5/7 Provisionando namespace + segredos com Terraform"
Push-Location infra
try {
    terraform init -input=false
    Assert "terraform init falhou"
    terraform apply -auto-approve -input=false `
        -var "postgres_password=oficina_local_dev" `
        -var "jwt_secret=LTMqSeW20V3t5aI4cig07ySQgJA-local-32bytes-min" `
        -var "webhook_secret=webhook-local-dev-secret-com-mais-de-32-caracteres" `
        -var "admin_email=admin@oficina.local" `
        -var "admin_password=Adm@QnB7Hhz77ZbVvwn8EWbaph1itMtkhZVv"
    Assert "terraform apply falhou"
}
finally {
    Pop-Location
}

# ---------------------------------------------------------------------------
Section "6/7 Aplicando os manifestos k8s (apontando para as imagens locais)"
# Renderiza o overlay local com o kustomize embutido no kubectl e substitui as
# referencias de imagem do GHCR (ghcr.io/...:2.0.0) pelas imagens locais
# carregadas no kind. Fazemos isso ANTES de aplicar porque o campo "image" de um
# Job (oficina-migration) e imutavel apos criado.
$rendered = kubectl --context $Ctx kustomize k8s/overlays/local
Assert "falha ao renderizar o kustomize"
$rendered = $rendered `
    -replace 'ghcr\.io/fiap-17soat-phantom/techchallenge-api:[^\s"]+',    "${ImageApi}:${Tag}" `
    -replace 'ghcr\.io/fiap-17soat-phantom/techchallenge-worker:[^\s"]+', "${ImageWorker}:${Tag}"
$manifest = New-TemporaryFile
Set-Content -Path $manifest -Value $rendered -Encoding utf8
kubectl --context $Ctx apply -f $manifest
Assert "falha ao aplicar os manifestos"
Remove-Item $manifest -ErrorAction SilentlyContinue

# ---------------------------------------------------------------------------
Section "7/7 Aguardando o deploy ficar pronto"
Write-Host ">>> PostgreSQL..." -ForegroundColor Yellow
kubectl --context $Ctx -n $Ns rollout status statefulset/postgres --timeout=240s
Assert "PostgreSQL nao ficou pronto"
Write-Host ">>> Migration..." -ForegroundColor Yellow
kubectl --context $Ctx -n $Ns wait --for=condition=complete job/oficina-migration --timeout=240s
Assert "migration nao concluiu"
Write-Host ">>> API..." -ForegroundColor Yellow
kubectl --context $Ctx -n $Ns rollout status deployment/oficina-api --timeout=240s
Assert "API nao ficou pronta"

Section "Pronto! Estado do cluster"
kubectl --context $Ctx -n $Ns get all
Write-Host ""
kubectl --context $Ctx -n $Ns get hpa

Write-Host ""
Write-Host "============================================================" -ForegroundColor Green
Write-Host " Aplicacao no ar no Kubernetes (kind)!" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host " Abrindo o acesso ao Swagger (port-forward) em uma nova janela..." -ForegroundColor Green

# Abre o port-forward numa nova janela do PowerShell, que fica aberta enquanto
# voce grava. Fechar a janela encerra o encaminhamento.
Start-Process powershell -ArgumentList @(
    "-NoExit", "-Command",
    "kubectl --context $Ctx -n $Ns port-forward svc/oficina-api 8080:80"
)

Start-Sleep -Seconds 3
Write-Host ""
Write-Host " Swagger:  http://localhost:8080/swagger" -ForegroundColor Green
Write-Host " Health:   http://localhost:8080/health/ready" -ForegroundColor Green
Write-Host ""
Write-Host " Login (Swagger -> Authorize): " -ForegroundColor Green
Write-Host "   email: admin@oficina.local" -ForegroundColor Green
Write-Host "   senha: Adm@QnB7Hhz77ZbVvwn8EWbaph1itMtkhZVv" -ForegroundColor Green
Write-Host ""
Write-Host " Para destruir tudo depois:  ./demo-k8s.ps1 -Destroy" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
