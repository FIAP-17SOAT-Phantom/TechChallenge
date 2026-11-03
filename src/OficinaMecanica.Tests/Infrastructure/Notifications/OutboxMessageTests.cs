using OficinaMecanica.Domain.Oficina.Enums;
using OficinaMecanica.Domain.Oficina.Events;
using OficinaMecanica.Infrastructure.Notifications;

namespace OficinaMecanica.Tests.Infrastructure.Notifications;

public sealed class OutboxMessageTests
{
    [Fact]
    public void FromDomainEvent_DevePreservarIdentidadeTipoPayloadEData()
    {
        var domainEvent = new OrdemDeServicoStatusAlteradoEvent(Guid.NewGuid(), "OS-0001", StatusOS.Recebida, StatusOS.EmDiagnostico);

        var message = OutboxMessage.FromDomainEvent(domainEvent);

        Assert.Equal(domainEvent.EventId, message.Id);
        Assert.Equal(nameof(OrdemDeServicoStatusAlteradoEvent), message.Type);
        Assert.Equal(domainEvent.OccurredOn, message.OccurredOnUtc);
        Assert.Contains("OS-0001", message.Payload);
        Assert.Equal(domainEvent.OccurredOn, message.NextAttemptOnUtc);
    }

    [Fact]
    public void MarkProcessed_DeveLimparLockAgendamentoEErro()
    {
        var message = CriarMensagem();
        var workerId = Guid.NewGuid();
        message.Claim(workerId, DateTime.UtcNow.AddMinutes(1));
        message.MarkFailed("erro temporario", DateTime.UtcNow.AddSeconds(30));
        message.Claim(workerId, DateTime.UtcNow.AddMinutes(1));
        var processedAt = DateTime.UtcNow;

        message.MarkProcessed(processedAt);

        Assert.Equal(processedAt, message.ProcessedOnUtc);
        Assert.Null(message.NextAttemptOnUtc);
        Assert.Null(message.LockedBy);
        Assert.Null(message.LockedUntilUtc);
        Assert.Null(message.LastError);
        Assert.Null(message.DeadLetterOnUtc);
    }

    [Fact]
    public void MarkFailed_ComNovaTentativa_DeveIncrementarELiberarLock()
    {
        var message = CriarMensagem();
        message.Claim(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(1));
        var retryAt = DateTime.UtcNow.AddSeconds(30);

        message.MarkFailed("falha transitoria", retryAt);

        Assert.Equal(1, message.Attempts);
        Assert.Equal("falha transitoria", message.LastError);
        Assert.Equal(retryAt, message.NextAttemptOnUtc);
        Assert.Null(message.DeadLetterOnUtc);
        Assert.Null(message.LockedBy);
        Assert.Null(message.LockedUntilUtc);
    }

    [Fact]
    public void MarkFailed_SemNovaTentativa_DeveEnviarParaDeadLetterETruncarErro()
    {
        var message = CriarMensagem();

        message.MarkFailed(new string('x', 2500), null);

        Assert.Equal(1, message.Attempts);
        Assert.Equal(2000, message.LastError!.Length);
        Assert.Null(message.NextAttemptOnUtc);
        Assert.NotNull(message.DeadLetterOnUtc);
    }

    private static OutboxMessage CriarMensagem() => OutboxMessage.FromDomainEvent(new OrdemDeServicoStatusAlteradoEvent(Guid.NewGuid(), "OS-0001", StatusOS.Recebida, StatusOS.EmDiagnostico));
}
