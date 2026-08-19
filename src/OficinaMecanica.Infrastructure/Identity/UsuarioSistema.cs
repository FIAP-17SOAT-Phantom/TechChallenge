using Microsoft.AspNetCore.Identity;

namespace OficinaMecanica.Infrastructure.Identity;

public sealed class UsuarioSistema : IdentityUser
{
    public Guid? ClienteId { get; set; }
}
