using System.Net;

namespace OficinaMecanica.IntegrationTests;

/// <summary>
/// Verifica que rotas protegidas exigem autenticacao JWT.
/// A API usa FallbackPolicy exigindo usuario autenticado por padrao.
/// </summary>
public sealed class AutorizacaoIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AutorizacaoIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AcessarRotaProtegida_SemToken_DeveRetornar401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/servicos");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AcessarRotaProtegida_ComTokenInvalido_DeveRetornar401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "token-invalido-qualquer");

        var response = await client.GetAsync("/api/servicos");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
