using OficinaMecanica.Domain.CatalogoServicos.Entities;

namespace OficinaMecanica.Tests.Domain.CatalogoServicos;

public sealed class ServicoTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveCriarServicoAtivo()
    {
        var servico = new Servico("Troca de oleo", "Troca completa", 120m, 60);

        Assert.Equal("Troca de oleo", servico.Nome);
        Assert.Equal("Troca completa", servico.Descricao);
        Assert.Equal(120m, servico.PrecoBase);
        Assert.Equal(60, servico.TempoEstimadoMinutos);
        Assert.True(servico.Ativo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_ComNomeInvalido_DeveLancarExcecao(string nome)
    {
        Assert.Throws<ArgumentException>(() => new Servico(nome, "desc", 100m, 60));
    }

    [Fact]
    public void Criar_ComPrecoNegativo_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() => new Servico("Servico", "desc", -1m, 60));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Criar_ComTempoInvalido_DeveLancarExcecao(int tempo)
    {
        Assert.Throws<ArgumentException>(() => new Servico("Servico", "desc", 100m, tempo));
    }

    [Fact]
    public void Atualizar_ComDadosValidos_DeveAtualizarCampos()
    {
        var servico = new Servico("Troca de oleo", "desc", 120m, 60);

        servico.Atualizar("Alinhamento", "Alinhamento 3D", 150m, 90);

        Assert.Equal("Alinhamento", servico.Nome);
        Assert.Equal("Alinhamento 3D", servico.Descricao);
        Assert.Equal(150m, servico.PrecoBase);
        Assert.Equal(90, servico.TempoEstimadoMinutos);
    }

    [Fact]
    public void Atualizar_ComPrecoNegativo_DeveLancarExcecao()
    {
        var servico = new Servico("Troca de oleo", "desc", 120m, 60);

        Assert.Throws<ArgumentException>(() => servico.Atualizar("Servico", "desc", -5m, 60));
    }

    [Fact]
    public void Desativar_DeveMarcarComoInativo()
    {
        var servico = new Servico("Troca de oleo", "desc", 120m, 60);

        servico.Desativar();

        Assert.False(servico.Ativo);
    }

    [Fact]
    public void Ativar_DeveMarcarComoAtivo()
    {
        var servico = new Servico("Troca de oleo", "desc", 120m, 60);
        servico.Desativar();

        servico.Ativar();

        Assert.True(servico.Ativo);
    }
}
