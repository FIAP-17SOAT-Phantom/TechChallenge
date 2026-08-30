using OficinaMecanica.Domain.Orcamentacao.Entities;
using OficinaMecanica.Domain.Orcamentacao.Enums;
using OficinaMecanica.Domain.Orcamentacao.ValueObjects;

namespace OficinaMecanica.Tests.Domain.Orcamentacao;

public sealed class OrcamentoItensTests
{
    [Fact]
    public void Criar_SemItens_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            new Orcamento(Guid.NewGuid(), 1, new List<ItemOrcamento>()));
    }

    [Fact]
    public void AdicionarItem_QuandoPendente_DeveAdicionar()
    {
        var orcamento = CriarOrcamento();
        var novoItem = new ItemOrcamento("Peca nova", TipoItem.Peca, 1, 40m, pecaId: Guid.NewGuid());

        var result = orcamento.AdicionarItem(novoItem);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, orcamento.Itens.Count);
    }

    [Fact]
    public void AdicionarItem_QuandoEnviado_DeveFalhar()
    {
        var orcamento = CriarOrcamento();
        orcamento.Enviar();
        var novoItem = new ItemOrcamento("Peca nova", TipoItem.Peca, 1, 40m, pecaId: Guid.NewGuid());

        var result = orcamento.AdicionarItem(novoItem);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void AlterarQuantidadeItem_ComItemExistente_DeveAtualizar()
    {
        var pecaId = Guid.NewGuid();
        var orcamento = new Orcamento(Guid.NewGuid(), 1, new List<ItemOrcamento>
        {
            new("Servico", TipoItem.Servico, 1, 100m, servicoId: Guid.NewGuid()),
            new("Peca", TipoItem.Peca, 1, 50m, pecaId: pecaId)
        });

        var result = orcamento.AlterarQuantidadeItem(TipoItem.Peca, pecaId, 5);

        Assert.True(result.IsSuccess);
        var item = orcamento.Itens.First(i => i.PecaId == pecaId);
        Assert.Equal(5, item.Quantidade);
    }

    [Fact]
    public void AlterarQuantidadeItem_ComQuantidadeInvalida_DeveFalhar()
    {
        var pecaId = Guid.NewGuid();
        var orcamento = new Orcamento(Guid.NewGuid(), 1, new List<ItemOrcamento>
        {
            new("Peca", TipoItem.Peca, 1, 50m, pecaId: pecaId)
        });

        var result = orcamento.AlterarQuantidadeItem(TipoItem.Peca, pecaId, 0);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void AlterarQuantidadeItem_ComItemInexistente_DeveFalhar()
    {
        var orcamento = CriarOrcamento();

        var result = orcamento.AlterarQuantidadeItem(TipoItem.Peca, Guid.NewGuid(), 3);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void RemoverItem_ComMaisDeUmItem_DeveRemover()
    {
        var pecaId = Guid.NewGuid();
        var orcamento = new Orcamento(Guid.NewGuid(), 1, new List<ItemOrcamento>
        {
            new("Servico", TipoItem.Servico, 1, 100m, servicoId: Guid.NewGuid()),
            new("Peca", TipoItem.Peca, 1, 50m, pecaId: pecaId)
        });

        var result = orcamento.RemoverItem(TipoItem.Peca, pecaId);

        Assert.True(result.IsSuccess);
        Assert.Single(orcamento.Itens);
    }

    [Fact]
    public void RemoverItem_ComItemUnico_DeveFalhar()
    {
        var servicoId = Guid.NewGuid();
        var orcamento = new Orcamento(Guid.NewGuid(), 1, new List<ItemOrcamento>
        {
            new("Servico", TipoItem.Servico, 1, 100m, servicoId: servicoId)
        });

        var result = orcamento.RemoverItem(TipoItem.Servico, servicoId);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void RemoverItem_UltimoServico_DeveFalhar()
    {
        var servicoId = Guid.NewGuid();
        var orcamento = new Orcamento(Guid.NewGuid(), 1, new List<ItemOrcamento>
        {
            new("Servico", TipoItem.Servico, 1, 100m, servicoId: servicoId),
            new("Peca", TipoItem.Peca, 1, 50m, pecaId: Guid.NewGuid())
        });

        var result = orcamento.RemoverItem(TipoItem.Servico, servicoId);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Enviar_SemServico_DeveFalhar()
    {
        var orcamento = new Orcamento(Guid.NewGuid(), 1, new List<ItemOrcamento>
        {
            new("Peca", TipoItem.Peca, 1, 50m, pecaId: Guid.NewGuid())
        });

        var result = orcamento.Enviar();

        Assert.True(result.IsFailure);
        Assert.Equal(StatusOrcamento.Pendente, orcamento.Status);
    }

    private static Orcamento CriarOrcamento() =>
        new(Guid.NewGuid(), 1, new List<ItemOrcamento>
        {
            new("Servico", TipoItem.Servico, 1, 100m, servicoId: Guid.NewGuid())
        });
}
