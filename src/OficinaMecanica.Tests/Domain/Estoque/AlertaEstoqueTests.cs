using OficinaMecanica.Domain.Estoque.Entities;

namespace OficinaMecanica.Tests.Domain.Estoque;

public sealed class AlertaEstoqueTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveCriarAlerta()
    {
        var pecaId = Guid.NewGuid();
        var alerta = new AlertaEstoque(pecaId, "Filtro de oleo", 2, 5);

        Assert.Equal(pecaId, alerta.PecaId);
        Assert.Equal("Filtro de oleo", alerta.NomePeca);
        Assert.Equal(2, alerta.QuantidadeDisponivel);
        Assert.Equal(5, alerta.QuantidadeMinima);
        Assert.False(alerta.Visualizado);
        Assert.False(alerta.Resolvido);
    }

    [Fact]
    public void Criar_ComPecaVazia_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            new AlertaEstoque(Guid.Empty, "Filtro", 2, 5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_ComNomeInvalido_DeveLancarExcecao(string nome)
    {
        Assert.Throws<ArgumentException>(() =>
            new AlertaEstoque(Guid.NewGuid(), nome, 2, 5));
    }

    [Fact]
    public void AtualizarQuantidade_DeveAtualizarValores()
    {
        var alerta = new AlertaEstoque(Guid.NewGuid(), "Filtro", 2, 5);

        alerta.AtualizarQuantidade(1, 8);

        Assert.Equal(1, alerta.QuantidadeDisponivel);
        Assert.Equal(8, alerta.QuantidadeMinima);
    }

    [Fact]
    public void MarcarComoVisualizado_DeveDefinirDataVisualizacao()
    {
        var alerta = new AlertaEstoque(Guid.NewGuid(), "Filtro", 2, 5);

        var result = alerta.MarcarComoVisualizado();

        Assert.True(result.IsSuccess);
        Assert.True(alerta.Visualizado);
    }

    [Fact]
    public void Resolver_DeveDefinirDataResolucaoEVisualizacao()
    {
        var alerta = new AlertaEstoque(Guid.NewGuid(), "Filtro", 2, 5);

        var result = alerta.Resolver();

        Assert.True(result.IsSuccess);
        Assert.True(alerta.Resolvido);
        Assert.True(alerta.Visualizado);
    }

    [Fact]
    public void Resolver_QuandoJaResolvido_DeveFalhar()
    {
        var alerta = new AlertaEstoque(Guid.NewGuid(), "Filtro", 2, 5);
        alerta.Resolver();

        var result = alerta.Resolver();

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void MarcarComoVisualizado_QuandoResolvido_DeveFalhar()
    {
        var alerta = new AlertaEstoque(Guid.NewGuid(), "Filtro", 2, 5);
        alerta.Resolver();

        var result = alerta.MarcarComoVisualizado();

        Assert.True(result.IsFailure);
    }
}
