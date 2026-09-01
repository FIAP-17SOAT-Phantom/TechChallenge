using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Infrastructure.Persistence;

namespace OficinaMecanica.Infrastructure.Identity;

public static class IdentitySeeder
{
    private static readonly string[] Roles = ["Admin", "Atendente", "Mecanico", "Cliente"];

    public static async Task SeedIdentityAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UsuarioSistema>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        await SeedRolesAsync(roleManager);

        var seedUsers = configuration.GetSection("Authentication:SeedUsers").GetChildren().Select(section => new SeedUserOptions
        {
            Email = section["Email"] ?? string.Empty,
            Password = section["Password"] ?? string.Empty,
            Role = section["Role"] ?? string.Empty,
            ClienteId = Guid.TryParse(section["ClienteId"], out var clienteId) ? clienteId : null
        }).ToList();

        foreach (var seedUser in seedUsers.Where(IsValid))
        {
            await SeedUserAsync(userManager, seedUser);
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static bool IsValid(SeedUserOptions seedUser) => !string.IsNullOrWhiteSpace(seedUser.Email) && !string.IsNullOrWhiteSpace(seedUser.Password) && Roles.Contains(seedUser.Role);

    private static async Task SeedUserAsync(UserManager<UsuarioSistema> userManager, SeedUserOptions seedUser)
    {
        var usuario = await userManager.FindByEmailAsync(seedUser.Email);

        if (usuario is null)
        {
            usuario = new UsuarioSistema { UserName = seedUser.Email, Email = seedUser.Email, EmailConfirmed = true, ClienteId = seedUser.ClienteId };
            var creationResult = await userManager.CreateAsync(usuario, seedUser.Password);

            if (!creationResult.Succeeded)
            {
                throw new InvalidOperationException($"Nao foi possivel criar o usuario inicial {seedUser.Email}: {string.Join(", ", creationResult.Errors.Select(error => error.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(usuario, seedUser.Role))
        {
            await userManager.AddToRoleAsync(usuario, seedUser.Role);
        }
    }
}

public sealed class SeedUserOptions
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public Guid? ClienteId { get; init; }
}
