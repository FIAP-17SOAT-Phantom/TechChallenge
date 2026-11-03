using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Orcamentacao.Enums;

namespace OficinaMecanica.Infrastructure.Integracoes;

public sealed class HmacWebhookOrcamentoAuthenticator : IWebhookOrcamentoAuthenticator
{
    private readonly WebhookOrcamentoOptions _options;

    public HmacWebhookOrcamentoAuthenticator(IOptions<WebhookOrcamentoOptions> options)
    {
        _options = options.Value;
    }

    public Result Validate(string externalEventId, long timestampUnix, Guid orcamentoId, RespostaOrcamentoExterna resposta, string? signature)
    {
        if (Encoding.UTF8.GetByteCount(_options.Secret) < 32)
            return Result.Unauthorized("Webhook de orcamento nao configurado");

        if (string.IsNullOrWhiteSpace(signature))
            return Result.Unauthorized("Assinatura do webhook ausente");

        DateTimeOffset timestamp;

        try
        {
            timestamp = DateTimeOffset.FromUnixTimeSeconds(timestampUnix);
        }
        catch (ArgumentOutOfRangeException)
        {
            return Result.Unauthorized("Timestamp do webhook invalido");
        }

        if ((DateTimeOffset.UtcNow - timestamp).Duration() > TimeSpan.FromSeconds(Math.Max(30, _options.ToleranceSeconds)))
            return Result.Unauthorized("Timestamp do webhook expirado ou futuro");

        var providedHex = signature.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase) ? signature[7..] : signature;
        byte[] providedSignature;

        try
        {
            providedSignature = Convert.FromHexString(providedHex);
        }
        catch (FormatException)
        {
            return Result.Unauthorized("Assinatura do webhook invalida");
        }

        var canonicalPayload = $"{externalEventId}\n{timestampUnix}\n{orcamentoId:D}\n{resposta.ToString().ToUpperInvariant()}";
        var expectedSignature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(_options.Secret), Encoding.UTF8.GetBytes(canonicalPayload));
        return providedSignature.Length == expectedSignature.Length && CryptographicOperations.FixedTimeEquals(providedSignature, expectedSignature) ? Result.Success() : Result.Unauthorized("Assinatura do webhook invalida");
    }
}
