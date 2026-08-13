using OficinaMecanica.Domain.Atendimento.Entities;
using OficinaMecanica.Domain.Atendimento.ValueObjects;

namespace OficinaMecanica.Application.Common.Interfaces;

public interface IClienteRepository : IRepository<Cliente>
{
 Task<Cliente?> GetByCpfAsync(Cpf cpf, CancellationToken cancellationToken = default);
 Task<IReadOnlyList<Cliente>> GetAllAsync(CancellationToken cancellationToken = default);
 Task<bool> ExistsByCpfAsync(Cpf cpf, CancellationToken cancellationToken = default);
}
