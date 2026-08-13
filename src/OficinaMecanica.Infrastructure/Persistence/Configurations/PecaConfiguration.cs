using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OficinaMecanica.Domain.Estoque.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Configurations;

public class PecaConfiguration : IEntityTypeConfiguration<Peca>
{
 public void Configure(EntityTypeBuilder<Peca> builder)
 {
 builder.ToTable("Pecas");
 builder.HasKey(p => p.Id);

 builder.Property(p => p.Nome).IsRequired().HasMaxLength(200);
 builder.Property(p => p.Codigo).IsRequired().HasMaxLength(50);
 builder.Property(p => p.Descricao).HasMaxLength(500);
 builder.Property(p => p.PrecoUnitario).IsRequired().HasPrecision(18, 2);
 builder.Property(p => p.QuantidadeEmEstoque).IsRequired();
 builder.Property(p => p.QuantidadeReservada).IsRequired();
 builder.Property(p => p.QuantidadeMinima).IsRequired();

 // QuantidadeDisponivel e calculado - nao persiste
 builder.Ignore(p => p.QuantidadeDisponivel);

 builder.HasIndex(p => p.Codigo).IsUnique();

 // Reserva e entity filha gerenciada pelo aggregate Peca
 // NAO tem DbSet proprio - acesso apenas via navegacao
 builder.HasMany(p => p.Reservas)
 .WithOne()
 .HasForeignKey(r => r.PecaId)
 .OnDelete(DeleteBehavior.Cascade);

 // Configuracao inline da Reserva (sem IEntityTypeConfiguration separado)
 builder.Navigation(p => p.Reservas).AutoInclude();

 builder.Ignore(p => p.DomainEvents);
 }
}

// Configuracao da Reserva como entity filha (inline, sem arquivo separado)
public class ReservaEntityConfiguration : IEntityTypeConfiguration<Reserva>
{
 public void Configure(EntityTypeBuilder<Reserva> builder)
 {
 builder.ToTable("Reservas");
 builder.HasKey(r => r.Id);

 builder.Property(r => r.PecaId).IsRequired();
 builder.Property(r => r.OrdemDeServicoId).IsRequired();
 builder.Property(r => r.Quantidade).IsRequired();
 builder.Property(r => r.DataReserva).IsRequired();

 builder.Property(r => r.Status)
 .IsRequired()
 .HasConversion<string>()
 .HasMaxLength(15);

 builder.HasIndex(r => r.OrdemDeServicoId);
 builder.HasIndex(r => r.PecaId);
 }
}
