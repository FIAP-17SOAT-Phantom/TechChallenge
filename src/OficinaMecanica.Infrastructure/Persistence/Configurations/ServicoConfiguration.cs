using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OficinaMecanica.Domain.CatalogoServicos.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Configurations;

public class ServicoConfiguration : IEntityTypeConfiguration<Servico>
{
    public void Configure(EntityTypeBuilder<Servico> builder)
    {
        builder.ToTable("Servicos");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Nome).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Descricao).HasMaxLength(500);
        builder.Property(s => s.PrecoBase).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.TempoEstimadoMinutos).IsRequired();
        builder.Property(s => s.Ativo).IsRequired();

        builder.Ignore(s => s.DomainEvents);
    }
}
