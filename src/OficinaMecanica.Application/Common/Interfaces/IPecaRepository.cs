using OficinaMecanica.Domain.Estoque.Entities;

namespace OficinaMecanica.Application.Common.Interfaces;

public interface IPecaRepository : IRepository<Peca>
{
    Task<Peca?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Peca>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Peca>> GetComEstoqueBaixoAsync(CancellationToken cancellationToken = default);
}
