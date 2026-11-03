using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Domain.Atendimento.Entities;
using OficinaMecanica.Domain.Atendimento.ValueObjects;
using OficinaMecanica.Domain.CatalogoServicos.Entities;
using OficinaMecanica.Domain.Estoque.Entities;
using OficinaMecanica.Domain.Oficina.Entities;
using OficinaMecanica.Domain.Oficina.Enums;
using OficinaMecanica.Domain.Orcamentacao.Entities;
using OficinaMecanica.Domain.Orcamentacao.Enums;
using OficinaMecanica.Domain.Orcamentacao.ValueObjects;
using OficinaMecanica.Infrastructure.Persistence;

namespace OficinaMecanica.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class Fase2IntegrationTests
{
    private static int _placaSequence = Random.Shared.Next(0, 900);
    private readonly CustomWebApplicationFactory _factory;

    public Fase2IntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthChecks_DevemDistinguirProcessoVivoEBancoPronto()
    {
        var client = _factory.CreateClient();
        var liveResponse = await client.GetAsync("/health/live");
        var readyResponse = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.OK, liveResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, readyResponse.StatusCode);
    }

    [Fact]
    public async Task WebhookAprovado_Repetido_DeveSerIdempotenteEAtualizarConsultaDeStatus()
    {
        var client = _factory.CreateClient();
        var cenario = await CriarCenarioComOrcamentoEnviadoAsync();
        var externalEventId = $"evento-{Guid.NewGuid():N}";
        var timestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var assinatura = CriarAssinatura(externalEventId, timestampUnix, cenario.OrcamentoId, "APROVADO");
        using var primeiraRequisicao = CriarRequisicaoWebhook(cenario.OrcamentoId, externalEventId, timestampUnix, assinatura);
        var primeiraResposta = await client.SendAsync(primeiraRequisicao);
        var primeiroResultado = await primeiraResposta.Content.ReadFromJsonAsync<RespostaWebhook>();

        Assert.Equal(HttpStatusCode.OK, primeiraResposta.StatusCode);
        Assert.NotNull(primeiroResultado);
        Assert.False(primeiroResultado!.Repetido);

        using var repeticao = CriarRequisicaoWebhook(cenario.OrcamentoId, externalEventId, timestampUnix, assinatura);
        var segundaResposta = await client.SendAsync(repeticao);
        var segundoResultado = await segundaResposta.Content.ReadFromJsonAsync<RespostaWebhook>();

        Assert.Equal(HttpStatusCode.OK, segundaResposta.StatusCode);
        Assert.NotNull(segundoResultado);
        Assert.True(segundoResultado!.Repetido);

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var ordemDeServico = await dbContext.OrdensDeServico.AsNoTracking().SingleAsync(ordem => ordem.Id == cenario.OrdemDeServicoId);
            var orcamento = await dbContext.Orcamentos.AsNoTracking().SingleAsync(item => item.Id == cenario.OrcamentoId);
            var peca = await dbContext.Pecas.Include(item => item.Reservas).AsNoTracking().SingleAsync(item => item.Id == cenario.PecaId);
            Assert.Equal(StatusOS.EmExecucao, ordemDeServico.Status);
            Assert.Equal(StatusOrcamento.Aprovado, orcamento.Status);
            var reserva = Assert.Single(peca.Reservas, item => item.OrdemDeServicoId == cenario.OrdemDeServicoId);
            Assert.Equal(2, reserva.Quantidade);
        }

        await AutenticarAdminAsync(client);
        var statusResponse = await client.GetAsync($"/api/ordens-de-servico/{cenario.OrdemDeServicoId}/status");
        var status = await statusResponse.Content.ReadFromJsonAsync<StatusResponse>();
        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);
        Assert.NotNull(status);
        Assert.Equal("EmExecucao", status!.Status);
        Assert.True(status.DataUltimaAtualizacao >= status.DataAbertura);
    }

    [Fact]
    public async Task ListarOrdensDeServico_DeveOrdenarPorPrioridadeEExcluirLogicamenteFinalizadasEEntregues()
    {
        var client = _factory.CreateClient();
        var numeros = await SemearOrdensParaListagemAsync();

        await AutenticarAdminAsync(client);
        var resposta = await client.GetAsync("/api/ordens-de-servico?tamanhoPagina=100");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var ordens = await resposta.Content.ReadFromJsonAsync<List<OrdemDeServicoResumo>>();
        Assert.NotNull(ordens);

        var relevantes = ordens!.Where(ordem => numeros.Contains(ordem.Numero)).ToList();

        // Exclusao logica: Finalizada e Entregue nao aparecem na listagem.
        Assert.DoesNotContain(relevantes, ordem => ordem.Status is "Finalizada" or "Entregue");

        // Ordenacao por prioridade de status.
        var statusEsperado = new[] { "EmExecucao", "AguardandoAprovacao", "EmDiagnostico", "Recebida" };
        var statusObtido = relevantes.Select(ordem => ordem.Status).ToList();
        Assert.Equal(statusEsperado, statusObtido);

        // Exclusao e logica, nao fisica: as 6 OS continuam no banco.
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var totalNoBanco = await dbContext.OrdensDeServico.CountAsync(ordem => numeros.Contains(ordem.Numero));
        Assert.Equal(6, totalNoBanco);
    }

    private async Task<HashSet<string>> SemearOrdensParaListagemAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sufixo = Guid.NewGuid().ToString("N")[..8];
        var numeros = new HashSet<string>();

        // Criadas fora de ordem de prioridade de proposito, para validar a ordenacao.
        var recebida = CriarOrdemComStatus($"OS-LST-REC-{sufixo}", StatusOS.Recebida);
        var emDiagnostico = CriarOrdemComStatus($"OS-LST-DIA-{sufixo}", StatusOS.EmDiagnostico);
        var aguardando = CriarOrdemComStatus($"OS-LST-AGU-{sufixo}", StatusOS.AguardandoAprovacao);
        var emExecucao = CriarOrdemComStatus($"OS-LST-EXE-{sufixo}", StatusOS.EmExecucao);
        var finalizada = CriarOrdemComStatus($"OS-LST-FIN-{sufixo}", StatusOS.Finalizada);
        var entregue = CriarOrdemComStatus($"OS-LST-ENT-{sufixo}", StatusOS.Entregue);

        foreach (var ordem in new[] { recebida, emDiagnostico, aguardando, emExecucao, finalizada, entregue })
        {
            await dbContext.OrdensDeServico.AddAsync(ordem);
            numeros.Add(ordem.Numero);
        }

        await dbContext.SaveChangesAsync();
        return numeros;
    }

    // Monta uma OS diretamente no status desejado percorrendo a maquina de estados do agregado.
    private static OrdemDeServico CriarOrdemComStatus(string numero, StatusOS statusDesejado)
    {
        var servicoId = Guid.NewGuid();
        var ordem = new OrdemDeServico(numero, Guid.NewGuid(), Guid.NewGuid());
        if (statusDesejado == StatusOS.Recebida)
        {
            return ordem;
        }

        Assert.True(ordem.IniciarDiagnostico(Guid.NewGuid()).IsSuccess);
        if (statusDesejado == StatusOS.EmDiagnostico)
        {
            return ordem;
        }

        Assert.True(ordem.RegistrarDiagnostico("Diagnostico de listagem", [new ItemOS(servicoId, null, 1)]).IsSuccess);
        if (statusDesejado == StatusOS.AguardandoAprovacao)
        {
            return ordem;
        }

        Assert.True(ordem.IniciarExecucao().IsSuccess);
        if (statusDesejado == StatusOS.EmExecucao)
        {
            return ordem;
        }

        Assert.True(ordem.RegistrarServicoExecutado(servicoId).IsSuccess);
        Assert.True(ordem.Finalizar().IsSuccess);
        if (statusDesejado == StatusOS.Finalizada)
        {
            return ordem;
        }

        Assert.True(ordem.RegistrarEntrega().IsSuccess);
        return ordem;
    }

    private async Task<CenarioWebhook> CriarCenarioComOrcamentoEnviadoAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sufixo = Guid.NewGuid().ToString("N")[..8];
        var cpf = Cpf.Criar(GerarCpfValido());
        var email = Email.Criar($"webhook.{sufixo}@teste.com");
        var placa = Placa.Criar(GerarPlaca());
        Assert.True(cpf.IsSuccess);
        Assert.True(email.IsSuccess);
        Assert.True(placa.IsSuccess);
        var cliente = new Cliente($"Cliente Webhook {sufixo}", cpf.Value, "11999999999", email.Value);
        var veiculo = new Veiculo(placa.Value, "Fiat", "Pulse", 2025, cliente.Id);
        var servico = new Servico($"Servico Webhook {sufixo}", "Servico para integracao", 180m, 60);
        var peca = new Peca($"Peca Webhook {sufixo}", $"WH-{sufixo}", "Peca para integracao", 50m, 20, 2);
        var ordemDeServico = new OrdemDeServico($"OS-WH-{sufixo}", cliente.Id, veiculo.Id);
        Assert.True(ordemDeServico.IniciarDiagnostico(Guid.NewGuid()).IsSuccess);
        Assert.True(ordemDeServico.RegistrarDiagnostico("Diagnostico de integracao", [new ItemOS(servico.Id, null, 1)]).IsSuccess);
        var orcamento = new Orcamento(ordemDeServico.Id, 1, [new ItemOrcamento(servico.Nome, TipoItem.Servico, 1, servico.PrecoBase, servicoId: servico.Id), new ItemOrcamento(peca.Nome, TipoItem.Peca, 2, peca.PrecoUnitario, pecaId: peca.Id)]);
        Assert.True(orcamento.Enviar().IsSuccess);
        Assert.True(ordemDeServico.VincularOrcamento(orcamento.Id).IsSuccess);

        await dbContext.Clientes.AddAsync(cliente);
        await dbContext.Veiculos.AddAsync(veiculo);
        await dbContext.Servicos.AddAsync(servico);
        await dbContext.Pecas.AddAsync(peca);
        await dbContext.OrdensDeServico.AddAsync(ordemDeServico);
        await dbContext.Orcamentos.AddAsync(orcamento);
        await dbContext.SaveChangesAsync();
        return new CenarioWebhook(ordemDeServico.Id, orcamento.Id, peca.Id);
    }

    private static HttpRequestMessage CriarRequisicaoWebhook(Guid orcamentoId, string externalEventId, long timestampUnix, string assinatura)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/webhooks/orcamentos/{orcamentoId}/resposta");
        request.Headers.TryAddWithoutValidation("X-Webhook-Signature", $"sha256={assinatura}");
        request.Content = JsonContent.Create(new { externalEventId, resposta = "Aprovado", timestampUnix });
        return request;
    }

    private static async Task AutenticarAdminAsync(HttpClient client)
    {
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { email = "admin@oficina.com", senha = "Admin@123456" });
        loginResponse.EnsureSuccessStatusCode();
        var token = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.Token);
    }

    private static string CriarAssinatura(string externalEventId, long timestampUnix, Guid orcamentoId, string resposta)
    {
        var payload = $"{externalEventId}\n{timestampUnix}\n{orcamentoId:D}\n{resposta}";
        var signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(CustomWebApplicationFactory.WebhookSecret), Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(signature).ToLowerInvariant();
    }

    private static string GerarCpfValido()
    {
        string baseCpf;

        do
        {
            baseCpf = Random.Shared.Next(100000000, 999999999).ToString();
        }
        while (baseCpf.Distinct().Count() == 1);

        var primeiroDigito = CalcularDigitoCpf(baseCpf, 10);
        var segundoDigito = CalcularDigitoCpf(baseCpf + primeiroDigito, 11);
        return baseCpf + primeiroDigito + segundoDigito;
    }

    private static int CalcularDigitoCpf(string numeros, int pesoInicial)
    {
        var soma = numeros.Select((caractere, indice) => (caractere - '0') * (pesoInicial - indice)).Sum();
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static string GerarPlaca()
    {
        var numero = Interlocked.Increment(ref _placaSequence) % 1000;
        return $"TST{numero / 100}A{numero % 100:D2}";
    }

    private sealed record OrdemDeServicoResumo(Guid Id, string Numero, string Status);
    private sealed record CenarioWebhook(Guid OrdemDeServicoId, Guid OrcamentoId, Guid PecaId);
    private sealed record RespostaWebhook(string ExternalEventId, bool Repetido);
    private sealed record StatusResponse(Guid Id, string Numero, string Status, DateTime DataUltimaAtualizacao, DateTime DataAbertura, DateTime? DataInicioExecucao, DateTime? DataFinalizacao);
    private sealed record TokenResponse(string Token, DateTime ExpiraEm, string Email, IReadOnlyList<string> Roles, bool TrocaSenhaObrigatoria);
}
