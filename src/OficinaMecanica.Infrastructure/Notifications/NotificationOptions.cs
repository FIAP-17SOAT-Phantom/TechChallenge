namespace OficinaMecanica.Infrastructure.Notifications;

public sealed class OutboxOptions
{
    public int PollingIntervalSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 20;
    public int MaxAttempts { get; set; } = 5;
    public int LockSeconds { get; set; } = 120;
}

public sealed class EmailOptions
{
    public string Provider { get; set; } = "Log";
    public string From { get; set; } = "Oficina Mecanica <nao-responda@exemplo.com>";
    public string? ResendApiKey { get; set; }
}
