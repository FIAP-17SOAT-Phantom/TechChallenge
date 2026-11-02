using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace OficinaMecanica.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class AberturaOrdemDeServicoIntegrationTests
{
    private readonly CustomWebApplicationFactory _factory;

    public AberturaOrdemDeServicoIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AbrirOrdemDeServico_ComClienteVeiculoServicoEPeca_DevePersistirSolicitacoes()
    {
        var client = await CriarClienteAutenticadoAsync();
        var catalogo = await CriarCatalogoAsync(client);
        var request = CriarAberturaRequest(catalogo, "52998224725", "cliente.abertura@teste.com", "BRA2E19");

        var aberturaResponse = await client.PostAsJsonAsync("/api/ordens-de-servico/abertura", request);

        Assert.Equal(HttpStatusCode.Created, aberturaResponse.StatusCode);
        var criada = await aberturaResponse.Content.ReadFromJsonAsync<OrdemDeServicoCriadaResponse>();
        Assert.NotNull(criada);
        Assert.NotEqual(Guid.Empty, criada!.Id);
        Assert.Matches("^OS-[0-9]{4,}$", criada.Numero);

        var consultaResponse = await client.GetAsync($"/api/ordens-de-servico/{criada.Id}");

        Assert.Equal(HttpStatusCode.OK, consultaResponse.StatusCode);
        var ordemDeServico = await consultaResponse.Content.ReadFromJsonAsync<OrdemDeServicoResponse>();
        Assert.NotNull(ordemDeServico);
        Assert.Equal(criada.Id, ordemDeServico!.Id);
        Assert.Equal(criada.Numero, ordemDeServico.Numero);
        Assert.Equal("Recebida", ordemDeServico.Status);

        var servicoSolicitado = Assert.Single(ordemDeServico.ServicosSolicitados);
        Assert.Equal(catalogo.ServicoId, servicoSolicitado.ServicoId);
        Assert.Equal(2, servicoSolicitado.Quantidade);
        Assert.Equal("Verificar ruido no motor", servicoSolicitado.Observacao);

        var pecaSolicitada = Assert.Single(ordemDeServico.PecasSolicitadas);
        Assert.Equal(catalogo.PecaId, pecaSolicitada.PecaId);
        Assert.Equal(3, pecaSolicitada.Quantidade);
        Assert.Equal("Utilizar durante a revisao", pecaSolicitada.Observacao);
    }

    [Fact]
    public async Task AbrirOrdemDeServico_ComPlacaDeOutroCliente_DeveRetornarConflict()
    {
        var client = await CriarClienteAutenticadoAsync();
        var catalogo = await CriarCatalogoAsync(client);
        var primeiraAbertura = CriarAberturaRequest(catalogo, "11144477735", "primeiro.cliente@teste.com", "ABC1D23");

        var primeiraResposta = await client.PostAsJsonAsync("/api/ordens-de-servico/abertura", primeiraAbertura);

        Assert.Equal(HttpStatusCode.Created, primeiraResposta.StatusCode);

        var segundaAbertura = CriarAberturaRequest(catalogo, "12345678909", "segundo.cliente@teste.com", "ABC1D23");
        var segundaResposta = await client.PostAsJsonAsync("/api/ordens-de-servico/abertura", segundaAbertura);

        Assert.Equal(HttpStatusCode.Conflict, segundaResposta.StatusCode);
    }

    private async Task<HttpClient> CriarClienteAutenticadoAsync()
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { email = "admin@oficina.com", senha = "Admin@123456" });

        loginResponse.EnsureSuccessStatusCode();
        var token = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.Token);

        return client;
    }

    private static async Task<CatalogoCriado> CriarCatalogoAsync(HttpClient client)
    {
        var sufixo = Guid.NewGuid().ToString("N")[..8];
        var servicoResponse = await client.PostAsJsonAsync("/api/servicos", new
        {
            nome = $"Revisao de abertura {sufixo}",
            descricao = "Servico criado pelo teste de abertura completa da OS",
            precoBase = 250.00m,
            tempoEstimadoMinutos = 90
        });

        Assert.Equal(HttpStatusCode.Created, servicoResponse.StatusCode);
        var servico = await servicoResponse.Content.ReadFromJsonAsync<RecursoCriadoResponse>();
        Assert.NotNull(servico);
        Assert.NotEqual(Guid.Empty, servico!.Id);

        var pecaResponse = await client.PostAsJsonAsync("/api/pecas", new
        {
            nome = $"Filtro de abertura {sufixo}",
            codigo = $"INT-{sufixo}",
            descricao = "Peca criada pelo teste de abertura completa da OS",
            precoUnitario = 75.00m,
            quantidadeEmEstoque = 20,
            quantidadeMinima = 5
        });

        Assert.Equal(HttpStatusCode.Created, pecaResponse.StatusCode);
        var peca = await pecaResponse.Content.ReadFromJsonAsync<RecursoCriadoResponse>();
        Assert.NotNull(peca);
        Assert.NotEqual(Guid.Empty, peca!.Id);

        return new CatalogoCriado(servico.Id, peca.Id);
    }

    private static AberturaRequest CriarAberturaRequest(CatalogoCriado catalogo, string cpf, string email, string placa) => new(new DadosClienteRequest("Cliente Integracao", cpf, "11999999999", email), new DadosVeiculoRequest(placa, "Honda", "Civic", 2024), [new ServicoSolicitadoRequest(catalogo.ServicoId, 2, "Verificar ruido no motor")], [new PecaSolicitadaRequest(catalogo.PecaId, 3, "Utilizar durante a revisao")]);

    private sealed record TokenResponse(string Token, DateTime ExpiraEm, string Email, IReadOnlyList<string> Roles, bool TrocaSenhaObrigatoria);
    private sealed record RecursoCriadoResponse(Guid Id);
    private sealed record CatalogoCriado(Guid ServicoId, Guid PecaId);
    private sealed record AberturaRequest(DadosClienteRequest Cliente, DadosVeiculoRequest Veiculo, IReadOnlyList<ServicoSolicitadoRequest> Servicos, IReadOnlyList<PecaSolicitadaRequest> Pecas);
    private sealed record DadosClienteRequest(string Nome, string Cpf, string Telefone, string Email);
    private sealed record DadosVeiculoRequest(string Placa, string Marca, string Modelo, int Ano);
    private sealed record ServicoSolicitadoRequest(Guid ServicoId, int Quantidade, string? Observacao);
    private sealed record PecaSolicitadaRequest(Guid PecaId, int Quantidade, string? Observacao);
    private sealed record OrdemDeServicoCriadaResponse(Guid Id, string Numero);
    private sealed record OrdemDeServicoResponse(Guid Id, string Numero, string Status, IReadOnlyList<ServicoSolicitadoResponse> ServicosSolicitados, IReadOnlyList<PecaSolicitadaResponse> PecasSolicitadas);
    private sealed record ServicoSolicitadoResponse(Guid ServicoId, int Quantidade, string? Observacao);
    private sealed record PecaSolicitadaResponse(Guid PecaId, int Quantidade, string? Observacao);
}
