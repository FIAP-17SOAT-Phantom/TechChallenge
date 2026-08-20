using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Oficina.Enums;

namespace OficinaMecanica.Domain.Oficina.Events;

public sealed record OrdemDeServicoCriadaEvent(Guid OrdemDeServicoId, string Numero) : DomainEvent;

public sealed record OrdemDeServicoFinalizadaEvent(Guid OrdemDeServicoId) : DomainEvent;

public sealed record OrdemDeServicoCanceladaEvent(Guid OrdemDeServicoId, StatusOS StatusAnterior) : DomainEvent;

public sealed record ServicoExecutadoEvent(Guid OrdemDeServicoId, Guid ServicoId, DateTime DataExecucao) : DomainEvent;
