using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Oficina.Enums;
using OficinaMecanica.Domain.Oficina.Events;
using OficinaMecanica.Domain.Orcamentacao.Events;
using OficinaMecanica.Infrastructure.Persistence;

namespace OficinaMecanica.Infrastructure.Notifications;

public sealed class OutboxProcessor : IOutboxProcessor
{
    private readonly AppDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly Guid _workerId = Guid.NewGuid();

    public OutboxProcessor(AppDbContext dbContext, IEmailSender emailSender, IOptions<OutboxOptions> options, ILogger<OutboxProcessor> logger)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        var processed = 0;
        var batchSize = Math.Clamp(_options.BatchSize, 1, 100);

        while (processed < batchSize)
        {
            var message = await ClaimPendingAsync(cancellationToken);

            if (message is null)
            {
                break;
            }

            await ProcessAsync(message, cancellationToken);
            processed++;
        }

        return processed;
    }

    private async Task<OutboxMessage?> ClaimPendingAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var lockedUntil = now.AddSeconds(Math.Max(10, _options.LockSeconds));
        var maxAttempts = Math.Max(1, _options.MaxAttempts);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var messages = await _dbContext.OutboxMessages.FromSqlInterpolated($$"""
            SELECT * FROM "OutboxMessages"
            WHERE "ProcessedOnUtc" IS NULL
              AND "DeadLetterOnUtc" IS NULL
              AND "Attempts" < {{maxAttempts}}
              AND ("NextAttemptOnUtc" IS NULL OR "NextAttemptOnUtc" <= {{now}})
              AND ("LockedUntilUtc" IS NULL OR "LockedUntilUtc" < {{now}})
            ORDER BY "OccurredOnUtc", "Id"
            LIMIT 1
            FOR UPDATE SKIP LOCKED
            """).ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            message.Claim(_workerId, lockedUntil);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return messages.SingleOrDefault();
    }

    private async Task ProcessAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var email = await ComposeEmailAsync(message, cancellationToken);

            if (email is null)
                throw new InvalidOperationException($"Evento {message.Type} do Outbox nao possui composicao de e-mail");

            await _emailSender.SendAsync(email, cancellationToken);

            message.MarkProcessed(DateTime.UtcNow);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            DateTime? nextAttempt = message.Attempts + 1 >= Math.Max(1, _options.MaxAttempts) ? null : DateTime.UtcNow.Add(CalculateBackoff(message.Attempts + 1));
            message.MarkFailed(exception.Message, nextAttempt);
            _logger.LogError(exception, "Falha ao processar mensagem Outbox {OutboxMessageId}. Tentativa {Attempt}", message.Id, message.Attempts);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<EmailMessage?> ComposeEmailAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        if (message.Type == nameof(OrdemDeServicoStatusAlteradoEvent))
        {
            var domainEvent = JsonSerializer.Deserialize<OrdemDeServicoStatusAlteradoEvent>(message.Payload) ?? throw new InvalidOperationException("Payload de status da OS invalido");
            var recipient = await GetOrderRecipientAsync(domainEvent.OrdemDeServicoId, cancellationToken);
            return new EmailMessage(recipient.Email, GetStatusSubject(domainEvent.Numero, domainEvent.StatusAtual), GetStatusBody(domainEvent.Numero, domainEvent.StatusAtual), message.Id.ToString());
        }

        if (message.Type == nameof(OrcamentoEnviadoEvent))
        {
            var domainEvent = JsonSerializer.Deserialize<OrcamentoEnviadoEvent>(message.Payload) ?? throw new InvalidOperationException("Payload de orcamento enviado invalido");
            var recipient = await GetOrderRecipientAsync(domainEvent.OrdemDeServicoId, cancellationToken);
            return new EmailMessage(recipient.Email, $"Orcamento da {recipient.Numero} disponivel", $"O orcamento da ordem de servico {recipient.Numero} esta aguardando sua aprovacao.", message.Id.ToString());
        }

        if (message.Type == nameof(OrcamentoAprovadoEvent))
        {
            var domainEvent = JsonSerializer.Deserialize<OrcamentoAprovadoEvent>(message.Payload) ?? throw new InvalidOperationException("Payload de orcamento aprovado invalido");
            var recipient = await GetOrderRecipientAsync(domainEvent.OrdemDeServicoId, cancellationToken);
            return new EmailMessage(recipient.Email, $"Orcamento da {recipient.Numero} aprovado", $"A aprovacao do orcamento da ordem de servico {recipient.Numero} foi registrada.", message.Id.ToString());
        }

        if (message.Type == nameof(OrcamentoRejeitadoEvent))
        {
            var domainEvent = JsonSerializer.Deserialize<OrcamentoRejeitadoEvent>(message.Payload) ?? throw new InvalidOperationException("Payload de orcamento rejeitado invalido");
            var recipient = await GetOrderRecipientAsync(domainEvent.OrdemDeServicoId, cancellationToken);
            return new EmailMessage(recipient.Email, $"Orcamento da {recipient.Numero} recusado", $"A recusa do orcamento da ordem de servico {recipient.Numero} foi registrada. A oficina entrara em contato para os proximos passos.", message.Id.ToString());
        }

        return null;
    }

    private async Task<OrderRecipient> GetOrderRecipientAsync(Guid ordemDeServicoId, CancellationToken cancellationToken)
    {
        var recipient = await (from ordemDeServico in _dbContext.OrdensDeServico
                               join cliente in _dbContext.Clientes on ordemDeServico.ClienteId equals cliente.Id
                               where ordemDeServico.Id == ordemDeServicoId
                               select new OrderRecipient(cliente.Email.Endereco, ordemDeServico.Numero)).AsNoTracking().SingleOrDefaultAsync(cancellationToken);

        return recipient ?? throw new InvalidOperationException($"Cliente da OS {ordemDeServicoId} nao encontrado");
    }

    private static string GetStatusSubject(string numero, StatusOS status) => $"{numero}: {GetStatusDescription(status)}";
    private static string GetStatusBody(string numero, StatusOS status) => $"A ordem de servico {numero} foi atualizada para: {GetStatusDescription(status)}.";
    private static string GetStatusDescription(StatusOS status) => status switch { StatusOS.Recebida => "Recebida", StatusOS.EmDiagnostico => "Em diagnostico", StatusOS.AguardandoAprovacao => "Aguardando aprovacao", StatusOS.EmExecucao => "Em execucao", StatusOS.Finalizada => "Finalizada", StatusOS.Entregue => "Entregue", StatusOS.Cancelada => "Cancelada", _ => status.ToString() };
    private static TimeSpan CalculateBackoff(int attempt) => TimeSpan.FromSeconds(Math.Min(Math.Pow(2, attempt) * 15, 3600));
    private sealed record OrderRecipient(string Email, string Numero);
}
