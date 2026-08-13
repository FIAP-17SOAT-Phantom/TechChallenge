using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Orcamentacao.Events;

public sealed record OrcamentoAprovadoEvent(Guid OrcamentoId, Guid OrdemDeServicoId) : DomainEvent;

public sealed record OrcamentoRejeitadoEvent(Guid OrcamentoId, Guid OrdemDeServicoId) : DomainEvent;

public sealed record OrcamentoEnviadoEvent(Guid OrcamentoId, Guid OrdemDeServicoId) : DomainEvent;
