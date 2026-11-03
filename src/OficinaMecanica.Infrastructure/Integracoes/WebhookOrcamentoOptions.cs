namespace OficinaMecanica.Infrastructure.Integracoes;

public sealed class WebhookOrcamentoOptions
{
    public string Secret { get; set; } = string.Empty;
    public int ToleranceSeconds { get; set; } = 300;
}
