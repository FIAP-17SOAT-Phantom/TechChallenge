using OficinaMecanica.Domain.Orcamentacao.Entities;

namespace OficinaMecanica.Application.Common.Interfaces;

public interface IOrcamentoRepository : IRepository<Orcamento>
{
    Task<Orcamento?> GetByOrdemDeServicoIdAsync(Guid ordemDeServicoId, CancellationToken cancellationToken = default);
    Task<int> GetVersaoAtualAsync(Guid ordemDeServicoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Orcamento>> GetPagedByClienteIdAsync(Guid clienteId, int page, int pageSize, CancellationToken cancellationToken = default);
}
