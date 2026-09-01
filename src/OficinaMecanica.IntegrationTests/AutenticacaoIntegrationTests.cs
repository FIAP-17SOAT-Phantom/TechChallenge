using System.Net;
using System.Net.Http.Json;

namespace OficinaMecanica.IntegrationTests;

/// <summary>
/// Testes de integracao da autenticacao contra um PostgreSQL real (TestContainers).
/// Exercitam o pipeline completo: HTTP -> Controller -> MediatR -> Identity -> banco.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public sealed class AutenticacaoIntegrationTests
{
    private readonly CustomWebApplicationFactory _factory;

    public AutenticacaoIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@oficina.com",
            senha = "Admin@123456"
        });

        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.NotNull(token);
        Assert.False(string.IsNullOrWhiteSpace(token!.Token));
        Assert.Contains("Admin", token.Roles);
    }

    [Fact]
    public async Task Login_ComSenhaIncorreta_DeveRetornarErro()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@oficina.com",
            senha = "SenhaErrada@999"
        });

        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Login_ComEmailInvalido_DeveRetornarBadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "nao-e-email",
            senha = "qualquer"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record TokenResponse(string Token, DateTime ExpiraEm, string Email, IReadOnlyList<string> Roles, bool TrocaSenhaObrigatoria);
}
