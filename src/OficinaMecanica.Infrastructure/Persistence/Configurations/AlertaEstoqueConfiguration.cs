using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OficinaMecanica.Domain.Estoque.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Configurations;

public sealed class AlertaEstoqueConfiguration : IEntityTypeConfiguration<AlertaEstoque>
{
    public void Configure(EntityTypeBuilder<AlertaEstoque> builder)
    {
        builder.ToTable("AlertasEstoque");
        builder.HasKey(alerta => alerta.Id);
        builder.Property(alerta => alerta.PecaId).IsRequired();
        builder.Property(alerta => alerta.NomePeca).IsRequired().HasMaxLength(200);
        builder.Property(alerta => alerta.QuantidadeDisponivel).IsRequired();
        builder.Property(alerta => alerta.QuantidadeMinima).IsRequired();
        builder.Property(alerta => alerta.DataCriacao).IsRequired();
        builder.Property(alerta => alerta.DataVisualizacao);
        builder.Property(alerta => alerta.DataResolucao);
        builder.Ignore(alerta => alerta.Visualizado);
        builder.Ignore(alerta => alerta.Resolvido);
        builder.Ignore(alerta => alerta.DomainEvents);
        builder.HasOne<Peca>().WithMany().HasForeignKey(alerta => alerta.PecaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(alerta => alerta.PecaId).IsUnique().HasFilter("\"DataResolucao\" IS NULL");
        builder.HasIndex(alerta => alerta.DataResolucao);
    }
}
