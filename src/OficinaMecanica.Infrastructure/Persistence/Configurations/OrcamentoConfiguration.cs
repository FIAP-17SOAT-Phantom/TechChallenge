using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OficinaMecanica.Domain.Orcamentacao.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Configurations;

public class OrcamentoConfiguration : IEntityTypeConfiguration<Orcamento>
{
    public void Configure(EntityTypeBuilder<Orcamento> builder)
    {
        builder.ToTable("Orcamentos");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrdemDeServicoId).IsRequired();
        builder.Property(o => o.Versao).IsRequired();

        builder.Property(o => o.Status)
        .IsRequired()
        .HasConversion<string>()
        .HasMaxLength(20);

        builder.Property(o => o.DataCriacao).IsRequired();
        builder.Property(o => o.Observacao).HasMaxLength(1000);

        // ValorTotal e calculado - nao persiste
        builder.Ignore(o => o.ValorTotal);

        // ItemOrcamento como owned collection com TODOS os campos mapeados
        builder.OwnsMany(o => o.Itens, item =>
        {
            item.ToTable("ItensOrcamento");
            item.WithOwner().HasForeignKey("OrcamentoId");
            item.Property(i => i.Descricao).IsRequired().HasMaxLength(200);
            item.Property(i => i.Tipo).IsRequired().HasConversion<string>().HasMaxLength(10);
            item.Property(i => i.Quantidade).IsRequired();
            item.Property(i => i.ValorUnitario).IsRequired().HasPrecision(18, 2);
            item.Property(i => i.PecaId);
            item.Property(i => i.ServicoId);
            item.Ignore(i => i.ValorTotal); // calculado
        });

        builder.HasIndex(o => o.OrdemDeServicoId);

        builder.Ignore(o => o.DomainEvents);
    }
}
