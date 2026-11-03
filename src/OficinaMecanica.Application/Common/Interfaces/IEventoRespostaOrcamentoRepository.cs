using OficinaMecanica.Domain.Orcamentacao.Entities;

namespace OficinaMecanica.Application.Common.Interfaces;

public interface IEventoRespostaOrcamentoRepository
{
    Task AcquireOrcamentoLockAsync(Guid orcamentoId, CancellationToken cancellationToken = default);
    Task<EventoRespostaOrcamento?> GetByExternalEventIdAsync(string externalEventId, CancellationToken cancellationToken = default);
    Task<bool> TryAddAsync(EventoRespostaOrcamento evento, CancellationToken cancellationToken = default);
}
