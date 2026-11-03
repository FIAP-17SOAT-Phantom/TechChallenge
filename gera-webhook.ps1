# Gera o corpo (JSON) e o header X-Webhook-Signature para o webhook de aprovacao de orcamento.
#
# Uso:
#   pwsh ./gera-webhook.ps1 -OrcamentoId "<id-do-orcamento>" -Secret "<WEBHOOK_ORCAMENTO_SECRET>"
#   pwsh ./gera-webhook.ps1 -OrcamentoId "<id>" -Secret "<segredo>" -Resposta Rejeitado
#
# O payload canonico assinado e: externalEventId \n timestampUnix \n orcamentoId \n RESPOSTA(maiusculo)

param(
  [Parameter(Mandatory = $true)][string]$OrcamentoId,
  [Parameter(Mandatory = $true)][string]$Secret,
  [ValidateSet("Aprovado", "Rejeitado")][string]$Resposta = "Aprovado"
)

$externalEventId = "evt-" + [guid]::NewGuid().ToString("N").Substring(0, 8)
$ts = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$payload = "$externalEventId`n$ts`n$OrcamentoId`n$($Resposta.ToUpperInvariant())"

$hmac = [System.Security.Cryptography.HMACSHA256]::new([Text.Encoding]::UTF8.GetBytes($Secret))
$hash = $hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($payload))
$sig = ([BitConverter]::ToString($hash) -replace '-', '').ToLower()

Write-Host ""
Write-Host "=== Corpo (cole no Swagger) ===" -ForegroundColor Cyan
[ordered]@{ externalEventId = $externalEventId; resposta = $Resposta; timestampUnix = $ts } | ConvertTo-Json

Write-Host ""
Write-Host "=== Header X-Webhook-Signature ===" -ForegroundColor Cyan
Write-Host "sha256=$sig"
Write-Host ""
Write-Host "Valido por ~300s. Use logo apos gerar." -ForegroundColor Yellow
