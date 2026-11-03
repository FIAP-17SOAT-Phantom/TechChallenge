using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Orcamentacao.Enums;
using OficinaMecanica.Infrastructure.Integracoes;

namespace OficinaMecanica.Tests.Infrastructure.Integracoes;

public sealed class HmacWebhookOrcamentoAuthenticatorTests
{
    private const string Secret = "segredo-de-teste-webhook-com-mais-de-32-bytes";

    [Fact]
    public void Validate_ComAssinaturaValida_DeveAutorizar()
    {
        var authenticator = CriarAuthenticator();
        var eventId = "evento-123";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var orcamentoId = Guid.NewGuid();
        var resposta = RespostaOrcamentoExterna.Aprovado;
        var signature = Assinar(eventId, timestamp, orcamentoId, resposta);

        var result = authenticator.Validate(eventId, timestamp, orcamentoId, resposta, $"sha256={signature}");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_ComAssinaturaInvalida_DeveRetornarUnauthorized()
    {
        var authenticator = CriarAuthenticator();

        var result = authenticator.Validate("evento-123", DateTimeOffset.UtcNow.ToUnixTimeSeconds(), Guid.NewGuid(), RespostaOrcamentoExterna.Rejeitado, new string('0', 64));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
    }

    [Fact]
    public void Validate_ComTimestampExpirado_DeveRetornarUnauthorized()
    {
        var authenticator = CriarAuthenticator(toleranceSeconds: 60);
        var eventId = "evento-expirado";
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-2).ToUnixTimeSeconds();
        var orcamentoId = Guid.NewGuid();
        var resposta = RespostaOrcamentoExterna.Aprovado;

        var result = authenticator.Validate(eventId, timestamp, orcamentoId, resposta, Assinar(eventId, timestamp, orcamentoId, resposta));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
        Assert.Contains("expirado", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_ComSegredoCurto_DeveRetornarUnauthorized()
    {
        var options = Options.Create(new WebhookOrcamentoOptions { Secret = "curto", ToleranceSeconds = 300 });
        var authenticator = new HmacWebhookOrcamentoAuthenticator(options);

        var result = authenticator.Validate("evento", DateTimeOffset.UtcNow.ToUnixTimeSeconds(), Guid.NewGuid(), RespostaOrcamentoExterna.Aprovado, new string('0', 64));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
        Assert.Contains("nao configurado", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    private static HmacWebhookOrcamentoAuthenticator CriarAuthenticator(int toleranceSeconds = 300) => new(Options.Create(new WebhookOrcamentoOptions { Secret = Secret, ToleranceSeconds = toleranceSeconds }));

    private static string Assinar(string externalEventId, long timestampUnix, Guid orcamentoId, RespostaOrcamentoExterna resposta)
    {
        var payload = $"{externalEventId}\n{timestampUnix}\n{orcamentoId:D}\n{resposta.ToString().ToUpperInvariant()}";
        var signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(Secret), Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(signature);
    }
}
