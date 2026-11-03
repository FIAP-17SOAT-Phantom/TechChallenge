namespace OficinaMecanica.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

public sealed record EmailMessage(string Recipient, string Subject, string TextBody, string? IdempotencyKey = null);
