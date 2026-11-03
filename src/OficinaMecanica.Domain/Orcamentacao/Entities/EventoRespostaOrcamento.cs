using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Orcamentacao.Enums;

namespace OficinaMecanica.Domain.Orcamentacao.Entities;

public sealed class EventoRespostaOrcamento : AggregateRoot
{
    public string ExternalEventId { get; private set; } = null!;
    public Guid OrcamentoId { get; private set; }
    public RespostaOrcamentoExterna Resposta { get; private set; }
    public long TimestampUnix { get; private set; }
    public DateTime RecebidoEmUtc { get; private set; }
    public DateTime ProcessadoEmUtc { get; private set; }
    public bool ProcessadoComSucesso { get; private set; }
    public ErrorType? TipoErro { get; private set; }
    public string? Erro { get; private set; }

    private EventoRespostaOrcamento() { }

    public EventoRespostaOrcamento(string externalEventId, Guid orcamentoId, RespostaOrcamentoExterna resposta, long timestampUnix) : this(externalEventId, orcamentoId, resposta, timestampUnix, Result.Success()) { }

    public EventoRespostaOrcamento(string externalEventId, Guid orcamentoId, RespostaOrcamentoExterna resposta, long timestampUnix, Result resultado)
    {
        if (string.IsNullOrWhiteSpace(externalEventId))
            throw new ArgumentException("ExternalEventId e obrigatorio");

        if (externalEventId.Length > 100)
            throw new ArgumentException("ExternalEventId deve ter no maximo 100 caracteres");

        if (orcamentoId == Guid.Empty)
            throw new ArgumentException("Orcamento e obrigatorio");

        ExternalEventId = externalEventId.Trim();
        OrcamentoId = orcamentoId;
        Resposta = resposta;
        TimestampUnix = timestampUnix;
        RecebidoEmUtc = DateTime.UtcNow;
        ProcessadoEmUtc = DateTime.UtcNow;
        ProcessadoComSucesso = resultado.IsSuccess;
        TipoErro = resultado.IsFailure ? resultado.ErrorType : null;
        Erro = resultado.IsFailure ? Limitar(resultado.Error, 1000) : null;
    }

    public bool Corresponde(Guid orcamentoId, RespostaOrcamentoExterna resposta, long timestampUnix) => OrcamentoId == orcamentoId && Resposta == resposta && TimestampUnix == timestampUnix;

    private static string Limitar(string value, int length) => value.Length <= length ? value : value[..length];
}
