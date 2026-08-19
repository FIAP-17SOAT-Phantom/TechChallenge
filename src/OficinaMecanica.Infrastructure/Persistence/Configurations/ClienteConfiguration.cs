using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OficinaMecanica.Domain.Atendimento.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome)
        .IsRequired()
        .HasMaxLength(200);

        builder.OwnsOne(c => c.Cpf, cpf =>
        {
            cpf.Property(x => x.Numero)
     .HasColumnName("Cpf")
     .IsRequired()
     .HasMaxLength(11);

            cpf.HasIndex(x => x.Numero).IsUnique();
        });

        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(x => x.Endereco)
     .HasColumnName("Email")
     .IsRequired()
     .HasMaxLength(255);
        });

        builder.Property(c => c.Telefone)
        .IsRequired()
        .HasMaxLength(20);

        // Ignorar Domain Events (nao persiste)
        builder.Ignore(c => c.DomainEvents);
    }
}
