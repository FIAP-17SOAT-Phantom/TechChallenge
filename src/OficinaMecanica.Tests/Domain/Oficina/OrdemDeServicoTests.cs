using OficinaMecanica.Domain.Oficina.Entities;
using OficinaMecanica.Domain.Oficina.Enums;
using OficinaMecanica.Domain.Oficina.Events;

namespace OficinaMecanica.Tests.Domain.Oficina;

public sealed class OrdemDeServicoTests
{
    [Fact]
    public void Criar_DeveIniciarComoRecebidaEEmitirEvento()
    {
        var ordemDeServico = CriarOrdemDeServico();

        Assert.Equal(StatusOS.Recebida, ordemDeServico.Status);
        Assert.Contains(ordemDeServico.DomainEvents, evento => evento is OrdemDeServicoCriadaEvent);
    }

    [Fact]
    public void FluxoCompleto_DeveRespeitarTransicoes()
    {
        var ordemDeServico = CriarOrdemDeServico();
        var item = new ItemOS(Guid.NewGuid(), null, 1);

        Assert.True(ordemDeServico.IniciarDiagnostico(Guid.NewGuid()).IsSuccess);
        Assert.Equal(StatusOS.EmDiagnostico, ordemDeServico.Status);
        Assert.True(ordemDeServico.RegistrarDiagnostico("Troca de oleo", [item]).IsSuccess);
        Assert.Equal(StatusOS.AguardandoAprovacao, ordemDeServico.Status);
        Assert.True(ordemDeServico.VincularOrcamento(Guid.NewGuid()).IsSuccess);
        Assert.True(ordemDeServico.IniciarExecucao().IsSuccess);
        Assert.Equal(StatusOS.EmExecucao, ordemDeServico.Status);
        Assert.True(ordemDeServico.Finalizar().IsSuccess);
        Assert.Equal(StatusOS.Finalizada, ordemDeServico.Status);
        Assert.True(ordemDeServico.RegistrarEntrega().IsSuccess);
        Assert.Equal(StatusOS.Entregue, ordemDeServico.Status);
    }

    [Fact]
    public void IniciarExecucao_QuandoRecebida_DeveFalhar()
    {
        var ordemDeServico = CriarOrdemDeServico();

        var result = ordemDeServico.IniciarExecucao();

        Assert.True(result.IsFailure);
        Assert.Equal(StatusOS.Recebida, ordemDeServico.Status);
    }

    [Fact]
    public void RegistrarDiagnostico_SemItens_DeveFalhar()
    {
        var ordemDeServico = CriarOrdemDeServico();
        ordemDeServico.IniciarDiagnostico(Guid.NewGuid());

        var result = ordemDeServico.RegistrarDiagnostico("Diagnostico", []);

        Assert.True(result.IsFailure);
        Assert.Equal(StatusOS.EmDiagnostico, ordemDeServico.Status);
    }

    [Fact]
    public void Cancelar_QuandoEmDiagnostico_DeveCancelarEEmitirEvento()
    {
        var ordemDeServico = CriarOrdemDeServico();
        ordemDeServico.IniciarDiagnostico(Guid.NewGuid());

        var result = ordemDeServico.Cancelar();

        Assert.True(result.IsSuccess);
        Assert.Equal(StatusOS.Cancelada, ordemDeServico.Status);
        Assert.Contains(ordemDeServico.DomainEvents, evento => evento is OrdemDeServicoCanceladaEvent);
    }

    [Theory]
    [InlineData(StatusOS.Finalizada)]
    [InlineData(StatusOS.Entregue)]
    public void Cancelar_QuandoEstadoTerminal_DeveFalhar(StatusOS status)
    {
        var ordemDeServico = CriarOrdemDeServico();
        ordemDeServico.IniciarDiagnostico(Guid.NewGuid());
        ordemDeServico.RegistrarDiagnostico("Diagnostico", [new ItemOS(Guid.NewGuid(), null, 1)]);
        ordemDeServico.IniciarExecucao();
        ordemDeServico.Finalizar();

        if (status == StatusOS.Entregue)
        {
            ordemDeServico.RegistrarEntrega();
        }

        var result = ordemDeServico.Cancelar();

        Assert.True(result.IsFailure);
        Assert.Equal(status, ordemDeServico.Status);
    }

    private static OrdemDeServico CriarOrdemDeServico() => new("OS-0001", Guid.NewGuid(), Guid.NewGuid());
}
