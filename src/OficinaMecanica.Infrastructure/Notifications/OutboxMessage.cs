using System.Text.Json;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Infrastructure.Notifications;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public DateTime? NextAttemptOnUtc { get; private set; }
    public DateTime? DeadLetterOnUtc { get; private set; }
    public int Attempts { get; private set; }
    public Guid? LockedBy { get; private set; }
    public DateTime? LockedUntilUtc { get; private set; }
    public string? LastError { get; private set; }

    private OutboxMessage() { }

    private OutboxMessage(Guid id, string type, string payload, DateTime occurredOnUtc)
    {
        Id = id;
        Type = type;
        Payload = payload;
        OccurredOnUtc = occurredOnUtc;
        NextAttemptOnUtc = occurredOnUtc;
    }

    public static OutboxMessage FromDomainEvent(IDomainEvent domainEvent) => new(domainEvent.EventId, domainEvent.GetType().Name, JsonSerializer.Serialize(domainEvent, domainEvent.GetType()), domainEvent.OccurredOn);

    public void Claim(Guid workerId, DateTime lockedUntilUtc)
    {
        LockedBy = workerId;
        LockedUntilUtc = lockedUntilUtc;
    }

    public void MarkProcessed(DateTime processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        NextAttemptOnUtc = null;
        LockedBy = null;
        LockedUntilUtc = null;
        LastError = null;
    }

    public void MarkFailed(string error, DateTime? nextAttemptOnUtc)
    {
        Attempts++;
        LastError = error.Length <= 2000 ? error : error[..2000];
        NextAttemptOnUtc = nextAttemptOnUtc;
        DeadLetterOnUtc = nextAttemptOnUtc.HasValue ? null : DateTime.UtcNow;
        LockedBy = null;
        LockedUntilUtc = null;
    }
}
