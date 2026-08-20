using OficinaMecanica.Domain.Atendimento.Entities;
using OficinaMecanica.Domain.Atendimento.ValueObjects;

namespace OficinaMecanica.Application.Common.Interfaces;

public interface IVeiculoRepository : IRepository<Veiculo>
{
    Task<Veiculo?> GetByPlacaAsync(Placa placa, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Veiculo>> GetByClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Veiculo>> GetPagedByClienteIdAsync(Guid clienteId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> ExistsByPlacaAsync(Placa placa, CancellationToken cancellationToken = default);
}
