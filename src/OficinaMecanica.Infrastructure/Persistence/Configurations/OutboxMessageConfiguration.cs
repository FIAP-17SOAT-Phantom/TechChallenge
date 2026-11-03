using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OficinaMecanica.Infrastructure.Notifications;

namespace OficinaMecanica.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Type).IsRequired().HasMaxLength(250);
        builder.Property(message => message.Payload).IsRequired().HasColumnType("jsonb");
        builder.Property(message => message.OccurredOnUtc).IsRequired();
        builder.Property(message => message.ProcessedOnUtc);
        builder.Property(message => message.NextAttemptOnUtc);
        builder.Property(message => message.DeadLetterOnUtc);
        builder.Property(message => message.Attempts).IsRequired();
        builder.Property(message => message.LockedBy);
        builder.Property(message => message.LockedUntilUtc);
        builder.Property(message => message.LastError).HasMaxLength(2000);
        builder.HasIndex(message => new { message.ProcessedOnUtc, message.DeadLetterOnUtc, message.NextAttemptOnUtc, message.Attempts });
        builder.HasIndex(message => message.LockedUntilUtc);
    }
}
