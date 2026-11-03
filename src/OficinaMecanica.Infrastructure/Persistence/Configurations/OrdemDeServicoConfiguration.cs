using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OficinaMecanica.Domain.Oficina.Entities;
using OficinaMecanica.Domain.Atendimento.Entities;
using OficinaMecanica.Domain.CatalogoServicos.Entities;
using OficinaMecanica.Domain.Estoque.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Configurations;

public class OrdemDeServicoConfiguration : IEntityTypeConfiguration<OrdemDeServico>
{
    public void Configure(EntityTypeBuilder<OrdemDeServico> builder)
    {
        builder.ToTable("OrdensDeServico");
        builder.HasKey(os => os.Id);

        builder.Property(os => os.Numero)
        .IsRequired()
        .HasMaxLength(20);

        builder.HasIndex(os => os.Numero).IsUnique();

        builder.Property(os => os.Status)
        .IsRequired()
        .HasConversion<string>()
        .HasMaxLength(30);

        builder.Property(os => os.ClienteId).IsRequired();
        builder.Property(os => os.VeiculoId).IsRequired();
        builder.Property(os => os.DataAbertura).IsRequired();
        builder.Property(os => os.DataUltimaAtualizacao).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(os => os.DataInicioExecucao);
        builder.Property(os => os.Diagnostico).HasMaxLength(2000);

        builder.HasOne<Cliente>().WithMany().HasForeignKey(os => os.ClienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Veiculo>().WithMany().HasForeignKey(os => os.VeiculoId).OnDelete(DeleteBehavior.Restrict);

        // ItemOS como owned collection com TODOS os campos mapeados
        builder.OwnsMany(os => os.Itens, item =>
        {
            item.ToTable("ItensOrdemDeServico");
            item.WithOwner().HasForeignKey("OrdemDeServicoId");
            item.Property(i => i.ServicoId).IsRequired();
            item.Property(i => i.PecaId);
            item.Property(i => i.Quantidade).IsRequired();
            item.Property(i => i.Observacao).HasMaxLength(500);
            item.Property(i => i.Executado).IsRequired();
            item.Property(i => i.DataExecucao);
            item.HasOne<Servico>().WithMany().HasForeignKey(i => i.ServicoId).OnDelete(DeleteBehavior.Restrict);
            item.HasOne<Peca>().WithMany().HasForeignKey(i => i.PecaId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.OwnsMany(os => os.ServicosSolicitados, item =>
        {
            item.ToTable("ServicosSolicitadosOrdemDeServico", table => table.HasCheckConstraint("CK_ServicosSolicitadosOrdemDeServico_Quantidade", "\"Quantidade\" > 0"));
            item.WithOwner().HasForeignKey("OrdemDeServicoId");
            item.Property(i => i.ServicoId).IsRequired();
            item.Property(i => i.Quantidade).IsRequired();
            item.Property(i => i.Observacao).HasMaxLength(500);
            item.HasOne<Servico>().WithMany().HasForeignKey(i => i.ServicoId).OnDelete(DeleteBehavior.Restrict);
            item.HasIndex("OrdemDeServicoId", nameof(ServicoSolicitadoOS.ServicoId)).IsUnique();
        });

        builder.OwnsMany(os => os.PecasSolicitadas, item =>
        {
            item.ToTable("PecasSolicitadasOrdemDeServico", table => table.HasCheckConstraint("CK_PecasSolicitadasOrdemDeServico_Quantidade", "\"Quantidade\" > 0"));
            item.WithOwner().HasForeignKey("OrdemDeServicoId");
            item.Property(i => i.PecaId).IsRequired();
            item.Property(i => i.Quantidade).IsRequired();
            item.Property(i => i.Observacao).HasMaxLength(500);
            item.HasOne<Peca>().WithMany().HasForeignKey(i => i.PecaId).OnDelete(DeleteBehavior.Restrict);
            item.HasIndex("OrdemDeServicoId", nameof(PecaSolicitadaOS.PecaId)).IsUnique();
        });

        builder.HasIndex(os => os.ClienteId);
        builder.HasIndex(os => os.VeiculoId);
        builder.HasIndex(os => os.Status);

        builder.Ignore(os => os.DomainEvents);
    }
}
