using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OficinaMecanica.Domain.Atendimento.Entities;
using OficinaMecanica.Infrastructure.Identity;

namespace OficinaMecanica.Infrastructure.Persistence.Configurations;

public sealed class UsuarioSistemaConfiguration : IEntityTypeConfiguration<UsuarioSistema>
{
    public void Configure(EntityTypeBuilder<UsuarioSistema> builder)
    {
        builder.HasOne<Cliente>().WithMany().HasForeignKey(usuario => usuario.ClienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(usuario => usuario.ClienteId).IsUnique().HasFilter("\"ClienteId\" IS NOT NULL");
    }
}
