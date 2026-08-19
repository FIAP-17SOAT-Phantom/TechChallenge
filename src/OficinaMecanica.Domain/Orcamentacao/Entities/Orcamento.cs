using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Orcamentacao.Enums;
using OficinaMecanica.Domain.Orcamentacao.Events;
using OficinaMecanica.Domain.Orcamentacao.ValueObjects;

namespace OficinaMecanica.Domain.Orcamentacao.Entities;

public class Orcamento : AggregateRoot
{
    public Guid OrdemDeServicoId { get; private set; }
    public int Versao { get; private set; }
    public StatusOrcamento Status { get; private set; }
    public decimal ValorTotal => _itens.Sum(i => i.ValorTotal);
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAprovacao { get; private set; }
    public string? Observacao { get; private set; }

    private readonly List<ItemOrcamento> _itens = new();
    public IReadOnlyCollection<ItemOrcamento> Itens => _itens.AsReadOnly();

    private Orcamento() { }

    public Orcamento(Guid ordemDeServicoId, int versao, List<ItemOrcamento> itens, string? observacao = null)
    {
        if (itens is null || itens.Count == 0)
            throw new ArgumentException("Orcamento deve ter pelo menos um item");

        OrdemDeServicoId = ordemDeServicoId;
        Versao = versao;
        Status = StatusOrcamento.Pendente;
        DataCriacao = DateTime.UtcNow;
        Observacao = observacao;
        _itens.AddRange(itens);
    }

    public Result Enviar()
    {
        if (Status != StatusOrcamento.Pendente)
            return Result.Failure("Orcamento deve estar Pendente para ser enviado");

        Status = StatusOrcamento.Enviado;
        RaiseDomainEvent(new OrcamentoEnviadoEvent(Id, OrdemDeServicoId));
        return Result.Success();
    }

    public Result Aprovar()
    {
        if (Status != StatusOrcamento.Enviado)
            return Result.Failure("Orcamento deve estar Enviado para ser aprovado");

        Status = StatusOrcamento.Aprovado;
        DataAprovacao = DateTime.UtcNow;
        RaiseDomainEvent(new OrcamentoAprovadoEvent(Id, OrdemDeServicoId));
        return Result.Success();
    }

    public Result Rejeitar()
    {
        if (Status != StatusOrcamento.Enviado)
            return Result.Failure("Orcamento deve estar Enviado para ser rejeitado");

        Status = StatusOrcamento.Rejeitado;
        RaiseDomainEvent(new OrcamentoRejeitadoEvent(Id, OrdemDeServicoId));
        return Result.Success();
    }

    public Result Cancelar()
    {
        if (Status == StatusOrcamento.Aprovado || Status == StatusOrcamento.Cancelado)
            return Result.Failure("Nao e possivel cancelar orcamento neste status");

        Status = StatusOrcamento.Cancelado;
        return Result.Success();
    }
}
