using OficinaMecanica.Domain.Oficina.Entities;
using OficinaMecanica.Domain.Oficina.Enums;

namespace OficinaMecanica.Tests.Domain.Oficina;

public sealed class SolicitacoesOrdemDeServicoTests
{
    [Fact]
    public void ServicoSolicitadoOS_ComDadosValidos_DeveNormalizarEManterDados()
    {
        var servicoId = Guid.NewGuid();

        var servico = new ServicoSolicitadoOS(servicoId, 2, "  Verificar vazamento  ");

        Assert.Equal(servicoId, servico.ServicoId);
        Assert.Equal(2, servico.Quantidade);
        Assert.Equal("Verificar vazamento", servico.Observacao);
    }

    [Fact]
    public void ServicoSolicitadoOS_ComServicoVazio_DeveLancarExcecao() => Assert.Throws<ArgumentException>(() => new ServicoSolicitadoOS(Guid.Empty, 1));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ServicoSolicitadoOS_ComQuantidadeInvalida_DeveLancarExcecao(int quantidade) => Assert.Throws<ArgumentException>(() => new ServicoSolicitadoOS(Guid.NewGuid(), quantidade));

    [Fact]
    public void ServicoSolicitadoOS_ComObservacaoAcimaDoLimite_DeveLancarExcecao() => Assert.Throws<ArgumentException>(() => new ServicoSolicitadoOS(Guid.NewGuid(), 1, new string('a', 501)));

    [Fact]
    public void PecaSolicitadaOS_ComDadosValidos_DeveNormalizarEManterDados()
    {
        var pecaId = Guid.NewGuid();

        var peca = new PecaSolicitadaOS(pecaId, 3, "  Modelo premium  ");

        Assert.Equal(pecaId, peca.PecaId);
        Assert.Equal(3, peca.Quantidade);
        Assert.Equal("Modelo premium", peca.Observacao);
    }

    [Fact]
    public void PecaSolicitadaOS_ComPecaVazia_DeveLancarExcecao() => Assert.Throws<ArgumentException>(() => new PecaSolicitadaOS(Guid.Empty, 1));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PecaSolicitadaOS_ComQuantidadeInvalida_DeveLancarExcecao(int quantidade) => Assert.Throws<ArgumentException>(() => new PecaSolicitadaOS(Guid.NewGuid(), quantidade));

    [Fact]
    public void PecaSolicitadaOS_ComObservacaoAcimaDoLimite_DeveLancarExcecao() => Assert.Throws<ArgumentException>(() => new PecaSolicitadaOS(Guid.NewGuid(), 1, new string('a', 501)));

    [Fact]
    public void RegistrarDiagnostico_DevePreservarServicosEPecasSolicitadosNaAbertura()
    {
        var servicoSolicitado = new ServicoSolicitadoOS(Guid.NewGuid(), 2, "Servico solicitado pelo cliente");
        var pecaSolicitada = new PecaSolicitadaOS(Guid.NewGuid(), 1, "Peca solicitada pelo cliente");
        var ordemDeServico = new OrdemDeServico("OS-0001", Guid.NewGuid(), Guid.NewGuid(), [servicoSolicitado], [pecaSolicitada]);
        var itemDoDiagnostico = new ItemOS(Guid.NewGuid(), Guid.NewGuid(), 1, "Item definido no diagnostico");

        var inicio = ordemDeServico.IniciarDiagnostico(Guid.NewGuid());
        var diagnostico = ordemDeServico.RegistrarDiagnostico("Diagnostico realizado", [itemDoDiagnostico]);

        Assert.True(inicio.IsSuccess);
        Assert.True(diagnostico.IsSuccess);
        Assert.Equal(StatusOS.AguardandoAprovacao, ordemDeServico.Status);
        Assert.Collection(ordemDeServico.ServicosSolicitados, item => Assert.Equal(servicoSolicitado, item));
        Assert.Collection(ordemDeServico.PecasSolicitadas, item => Assert.Equal(pecaSolicitada, item));
        Assert.Collection(ordemDeServico.Itens, item => Assert.Equal(itemDoDiagnostico, item));
    }
}
