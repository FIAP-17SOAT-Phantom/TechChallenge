using OficinaMecanica.Domain.Atendimento.Entities;
using OficinaMecanica.Domain.Atendimento.ValueObjects;

namespace OficinaMecanica.Tests.Domain.Atendimento;

public sealed class VeiculoTests
{
    private static Placa PlacaValida() => Placa.Criar("ABC1D23").Value;

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarVeiculo()
    {
        var clienteId = Guid.NewGuid();
        var veiculo = new Veiculo(PlacaValida(), "Fiat", "Uno", 2020, clienteId);

        Assert.Equal("Fiat", veiculo.Marca);
        Assert.Equal("Uno", veiculo.Modelo);
        Assert.Equal(2020, veiculo.Ano);
        Assert.Equal(clienteId, veiculo.ClienteId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_ComMarcaInvalida_DeveLancarExcecao(string marca)
    {
        Assert.Throws<ArgumentException>(() =>
            new Veiculo(PlacaValida(), marca, "Uno", 2020, Guid.NewGuid()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_ComModeloInvalido_DeveLancarExcecao(string modelo)
    {
        Assert.Throws<ArgumentException>(() =>
            new Veiculo(PlacaValida(), "Fiat", modelo, 2020, Guid.NewGuid()));
    }

    [Theory]
    [InlineData(1899)]
    [InlineData(3000)]
    public void Criar_ComAnoInvalido_DeveLancarExcecao(int ano)
    {
        Assert.Throws<ArgumentException>(() =>
            new Veiculo(PlacaValida(), "Fiat", "Uno", ano, Guid.NewGuid()));
    }

    [Fact]
    public void PertenceAoCliente_QuandoMesmoCliente_DeveRetornarTrue()
    {
        var clienteId = Guid.NewGuid();
        var veiculo = new Veiculo(PlacaValida(), "Fiat", "Uno", 2020, clienteId);

        Assert.True(veiculo.PertenceAoCliente(clienteId));
    }

    [Fact]
    public void PertenceAoCliente_QuandoClienteDiferente_DeveRetornarFalse()
    {
        var veiculo = new Veiculo(PlacaValida(), "Fiat", "Uno", 2020, Guid.NewGuid());

        Assert.False(veiculo.PertenceAoCliente(Guid.NewGuid()));
    }

    [Fact]
    public void Atualizar_ComDadosValidos_DeveAtualizarCampos()
    {
        var veiculo = new Veiculo(PlacaValida(), "Fiat", "Uno", 2020, Guid.NewGuid());

        veiculo.Atualizar("Volkswagen", "Gol", 2022);

        Assert.Equal("Volkswagen", veiculo.Marca);
        Assert.Equal("Gol", veiculo.Modelo);
        Assert.Equal(2022, veiculo.Ano);
    }

    [Fact]
    public void Atualizar_ComAnoInvalido_DeveLancarExcecao()
    {
        var veiculo = new Veiculo(PlacaValida(), "Fiat", "Uno", 2020, Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => veiculo.Atualizar("Fiat", "Uno", 1800));
    }
}
