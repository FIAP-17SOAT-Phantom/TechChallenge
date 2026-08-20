using OficinaMecanica.Domain.CatalogoServicos.Entities;

namespace OficinaMecanica.Application.Common.Interfaces;

public interface IServicoRepository : IRepository<Servico>
{
    Task<IReadOnlyList<Servico>> GetAllAtivosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Servico>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Servico>> GetPagedAsync(bool somenteAtivos, int page, int pageSize, CancellationToken cancellationToken = default);
}
