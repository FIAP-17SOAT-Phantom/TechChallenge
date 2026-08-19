using OficinaMecanica.Domain.Orcamentacao.Entities;
using OficinaMecanica.Domain.Orcamentacao.Enums;
using OficinaMecanica.Domain.Orcamentacao.Events;
using OficinaMecanica.Domain.Orcamentacao.ValueObjects;

namespace OficinaMecanica.Tests.Domain.Orcamentacao;

public sealed class OrcamentoTests
{
    [Fact]
    public void Criar_DeveCalcularValorTotalNoDomain()
    {
        var itens = new List<ItemOrcamento>
        {
            new("Servico", TipoItem.Servico, 2, 100m, servicoId: Guid.NewGuid()),
            new("Peca", TipoItem.Peca, 3, 50m, pecaId: Guid.NewGuid())
        };

        var orcamento = new Orcamento(Guid.NewGuid(), 1, itens);

        Assert.Equal(350m, orcamento.ValorTotal);
        Assert.Equal(StatusOrcamento.Pendente, orcamento.Status);
    }

    [Fact]
    public void EnviarEAprovar_DeveAlterarStatusEEmitirEventos()
    {
        var orcamento = CriarOrcamento();

        Assert.True(orcamento.Enviar().IsSuccess);
        Assert.Equal(StatusOrcamento.Enviado, orcamento.Status);
        Assert.Contains(orcamento.DomainEvents, evento => evento is OrcamentoEnviadoEvent);
        Assert.True(orcamento.Aprovar().IsSuccess);
        Assert.Equal(StatusOrcamento.Aprovado, orcamento.Status);
        Assert.NotNull(orcamento.DataAprovacao);
        Assert.Contains(orcamento.DomainEvents, evento => evento is OrcamentoAprovadoEvent);
    }

    [Fact]
    public void Aprovar_QuandoPendente_DeveFalhar()
    {
        var orcamento = CriarOrcamento();

        var result = orcamento.Aprovar();

        Assert.True(result.IsFailure);
        Assert.Equal(StatusOrcamento.Pendente, orcamento.Status);
    }

    [Fact]
    public void Rejeitar_QuandoEnviado_DeveAlterarStatus()
    {
        var orcamento = CriarOrcamento();
        orcamento.Enviar();

        var result = orcamento.Rejeitar();

        Assert.True(result.IsSuccess);
        Assert.Equal(StatusOrcamento.Rejeitado, orcamento.Status);
        Assert.Contains(orcamento.DomainEvents, evento => evento is OrcamentoRejeitadoEvent);
    }

    [Fact]
    public void Cancelar_QuandoPendente_DeveAlterarStatus()
    {
        var orcamento = CriarOrcamento();

        var result = orcamento.Cancelar();

        Assert.True(result.IsSuccess);
        Assert.Equal(StatusOrcamento.Cancelado, orcamento.Status);
    }

    [Fact]
    public void Cancelar_QuandoAprovado_DeveFalhar()
    {
        var orcamento = CriarOrcamento();
        orcamento.Enviar();
        orcamento.Aprovar();

        var result = orcamento.Cancelar();

        Assert.True(result.IsFailure);
        Assert.Equal(StatusOrcamento.Aprovado, orcamento.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ItemOrcamento_ComQuantidadeInvalida_DeveLancarExcecao(int quantidade)
    {
        Assert.Throws<ArgumentException>(() => new ItemOrcamento("Item", TipoItem.Peca, quantidade, 10m, pecaId: Guid.NewGuid()));
    }

    [Fact]
    public void ItemOrcamento_ComValorNegativo_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() => new ItemOrcamento("Item", TipoItem.Peca, 1, -1m, pecaId: Guid.NewGuid()));
    }

    private static Orcamento CriarOrcamento() => new(Guid.NewGuid(), 1, [new ItemOrcamento("Servico", TipoItem.Servico, 1, 100m, servicoId: Guid.NewGuid())]);
}
