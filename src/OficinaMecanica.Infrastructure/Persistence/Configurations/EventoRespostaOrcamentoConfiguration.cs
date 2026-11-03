using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OficinaMecanica.Domain.Orcamentacao.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Configurations;

public sealed class EventoRespostaOrcamentoConfiguration : IEntityTypeConfiguration<EventoRespostaOrcamento>
{
    public void Configure(EntityTypeBuilder<EventoRespostaOrcamento> builder)
    {
        builder.ToTable("EventosRespostaOrcamento");
        builder.HasKey(evento => evento.Id);
        builder.Property(evento => evento.ExternalEventId).IsRequired().HasMaxLength(100);
        builder.Property(evento => evento.OrcamentoId).IsRequired();
        builder.Property(evento => evento.Resposta).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(evento => evento.TimestampUnix).IsRequired();
        builder.Property(evento => evento.RecebidoEmUtc).IsRequired();
        builder.Property(evento => evento.ProcessadoEmUtc).IsRequired();
        builder.Property(evento => evento.ProcessadoComSucesso).IsRequired();
        builder.Property(evento => evento.TipoErro).HasConversion<string>().HasMaxLength(30);
        builder.Property(evento => evento.Erro).HasMaxLength(1000);
        builder.HasIndex(evento => evento.ExternalEventId).IsUnique();
        builder.HasIndex(evento => evento.OrcamentoId);
        builder.HasIndex(evento => evento.RecebidoEmUtc);
        builder.HasOne<Orcamento>().WithMany().HasForeignKey(evento => evento.OrcamentoId).OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(evento => evento.DomainEvents);
    }
}
