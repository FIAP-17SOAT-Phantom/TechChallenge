using OficinaMecanica.Domain.Estoque.Entities;

namespace OficinaMecanica.Application.Common.Interfaces;

public interface IAlertaEstoqueRepository : IRepository<AlertaEstoque>
{
    Task<AlertaEstoque?> GetAtivoByPecaIdAsync(Guid pecaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AlertaEstoque>> GetAllAsync(bool somenteAtivos, int page, int pageSize, CancellationToken cancellationToken = default);
}
