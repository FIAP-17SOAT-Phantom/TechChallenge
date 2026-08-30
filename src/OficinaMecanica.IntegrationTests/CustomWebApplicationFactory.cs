using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

namespace OficinaMecanica.IntegrationTests;

/// <summary>
/// Factory que hospeda a API em memoria apontando para um PostgreSQL real
/// provisionado via TestContainers (container Docker efemero por classe de teste).
/// As migrations e o seed do Identity rodam automaticamente no startup da API.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("oficina_mecanica_test")
        .WithUsername("oficina")
        .WithPassword("oficina_test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
                ["Jwt:Issuer"] = "OficinaMecanica.API",
                ["Jwt:Audience"] = "OficinaMecanica.Client",
                ["Jwt:Secret"] = "chave-secreta-de-testes-com-mais-de-32-bytes-1234567890",
                ["Jwt:ExpirationMinutes"] = "60",
                ["Authentication:SeedUsers:0:Email"] = "admin@oficina.com",
                ["Authentication:SeedUsers:0:Password"] = "Admin@123456",
                ["Authentication:SeedUsers:0:Role"] = "Admin"
            };

            config.AddInMemoryCollection(overrides);
        });
    }

    public async Task InitializeAsync() => await _postgres.StartAsync();

    public new async Task DisposeAsync() => await _postgres.DisposeAsync();
}
