using OficinaMecanica.Domain.Estoque.Entities;

namespace OficinaMecanica.Application.Common.Interfaces;

public interface IPecaRepository : IRepository<Peca>
{
    Task<Peca?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Peca>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Peca>> GetComEstoqueBaixoAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Peca>> GetPagedAsync(bool somenteEstoqueBaixo, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> HasReferencesAsync(Guid pecaId, CancellationToken cancellationToken = default);
}
