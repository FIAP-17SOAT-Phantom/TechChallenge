using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OficinaMecanica.Domain.Atendimento.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Configurations;

public class VeiculoConfiguration : IEntityTypeConfiguration<Veiculo>
{
    public void Configure(EntityTypeBuilder<Veiculo> builder)
    {
        builder.ToTable("Veiculos");
        builder.HasKey(v => v.Id);

        builder.OwnsOne(v => v.Placa, placa =>
        {
            placa.Property(x => x.Valor)
     .HasColumnName("Placa")
     .IsRequired()
     .HasMaxLength(7);

            placa.HasIndex(x => x.Valor).IsUnique();
        });

        builder.Property(v => v.Marca).IsRequired().HasMaxLength(50);
        builder.Property(v => v.Modelo).IsRequired().HasMaxLength(50);
        builder.Property(v => v.Ano).IsRequired();
        builder.Property(v => v.ClienteId).IsRequired();

        builder.HasIndex(v => v.ClienteId);

        builder.Ignore(v => v.DomainEvents);
    }
}
