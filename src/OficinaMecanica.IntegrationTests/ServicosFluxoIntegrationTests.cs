using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace OficinaMecanica.IntegrationTests;

/// <summary>
/// Fluxo de negocio ponta a ponta com PostgreSQL real:
/// login -> criar servico (persiste no banco) -> consultar servico criado.
/// Valida persistencia via EF Core + mapeamento + serializacao HTTP.
/// </summary>
public sealed class ServicosFluxoIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ServicosFluxoIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CriarEConsultarServico_ComoAdmin_DevePersistirNoBanco()
    {
        var client = await CriarClienteAutenticadoAsync();

        // Criar servico
        var criarResponse = await client.PostAsJsonAsync("/api/servicos", new
        {
            nome = "Troca de oleo",
            descricao = "Troca completa de oleo e filtro",
            precoBase = 150.00m,
            tempoEstimadoMinutos = 45
        });

        Assert.Equal(HttpStatusCode.Created, criarResponse.StatusCode);
        var criado = await criarResponse.Content.ReadFromJsonAsync<CriarServicoResponse>();
        Assert.NotNull(criado);
        Assert.NotEqual(Guid.Empty, criado!.Id);

        // Consultar o servico recem-criado (le do banco)
        var consultarResponse = await client.GetAsync($"/api/servicos/{criado.Id}");

        consultarResponse.EnsureSuccessStatusCode();
        var servico = await consultarResponse.Content.ReadFromJsonAsync<ServicoResponse>();

        Assert.NotNull(servico);
        Assert.Equal("Troca de oleo", servico!.Nome);
        Assert.Equal(150.00m, servico.PrecoBase);
    }

    [Fact]
    public async Task CriarServico_ComDadosInvalidos_DeveRetornarBadRequest()
    {
        var client = await CriarClienteAutenticadoAsync();

        var response = await client.PostAsJsonAsync("/api/servicos", new
        {
            nome = "",
            descricao = "sem nome",
            precoBase = -10m,
            tempoEstimadoMinutos = 0
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<HttpClient> CriarClienteAutenticadoAsync()
    {
        var client = _factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@oficina.com",
            senha = "Admin@123456"
        });

        loginResponse.EnsureSuccessStatusCode();
        var token = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.Token);

        return client;
    }

    private sealed record TokenResponse(string Token, DateTime ExpiraEm, string Email, IReadOnlyList<string> Roles, bool TrocaSenhaObrigatoria);
    private sealed record CriarServicoResponse(Guid Id);
    private sealed record ServicoResponse(Guid Id, string Nome, string Descricao, decimal PrecoBase, int TempoEstimadoMinutos, bool Ativo);
}
