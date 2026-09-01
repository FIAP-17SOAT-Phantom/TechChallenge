using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

namespace OficinaMecanica.IntegrationTests;

/// <summary>
/// Factory que hospeda a API em memoria apontando para um PostgreSQL real
/// provisionado via TestContainers (container Docker efemero compartilhado pela colecao de testes).
/// As migrations e o seed do Identity rodam automaticamente no startup da API.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly string[] ConfigurationKeys = ["ConnectionStrings__DefaultConnection", "Jwt__Issuer", "Jwt__Audience", "Jwt__Secret", "Jwt__ExpirationMinutes", "Authentication__SeedUsers__0__Email", "Authentication__SeedUsers__0__Password", "Authentication__SeedUsers__0__Role"];
    private readonly Dictionary<string, string?> _originalEnvironmentVariables = [];
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
            config.AddInMemoryCollection(CriarConfiguracoesTeste());
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var overrides = CriarConfiguracoesTeste();

        foreach (var key in ConfigurationKeys)
        {
            _originalEnvironmentVariables[key] = Environment.GetEnvironmentVariable(key);
            Environment.SetEnvironmentVariable(key, overrides[key.Replace("__", ":")]);
        }
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();

        foreach (var key in ConfigurationKeys)
        {
            Environment.SetEnvironmentVariable(key, _originalEnvironmentVariables[key]);
        }
    }

    private Dictionary<string, string?> CriarConfiguracoesTeste() => new()
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
}
