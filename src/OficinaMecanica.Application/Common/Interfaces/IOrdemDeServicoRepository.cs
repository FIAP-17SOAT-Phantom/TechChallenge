using OficinaMecanica.Domain.Oficina.Entities;
using OficinaMecanica.Domain.Oficina.Enums;

namespace OficinaMecanica.Application.Common.Interfaces;

public interface IOrdemDeServicoRepository : IRepository<OrdemDeServico>
{
 Task<OrdemDeServico?> GetByNumeroAsync(string numero, CancellationToken cancellationToken = default);
 Task<IReadOnlyList<OrdemDeServico>> GetByClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default);
 Task<IReadOnlyList<OrdemDeServico>> GetByStatusAsync(StatusOS status, CancellationToken cancellationToken = default);
 Task<IReadOnlyList<OrdemDeServico>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
 Task<string> GerarProximoNumeroAsync(CancellationToken cancellationToken = default);
}
