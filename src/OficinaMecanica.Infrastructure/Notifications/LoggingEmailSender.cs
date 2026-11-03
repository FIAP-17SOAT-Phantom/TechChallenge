using Microsoft.Extensions.Logging;
using OficinaMecanica.Application.Common.Interfaces;

namespace OficinaMecanica.Infrastructure.Notifications;

public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("E-mail simulado para {Recipient}. Assunto: {Subject}. Conteudo: {TextBody}", message.Recipient, message.Subject, message.TextBody);
        return Task.CompletedTask;
    }
}
