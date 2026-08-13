using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.Common.Interfaces;

/// <summary>
/// Repositorio generico por Aggregate Root.
/// Em DDD, repositorios operam sobre aggregates, nunca sobre entities filhas diretamente.
/// </summary>
public interface IRepository<T> where T : AggregateRoot
{
 Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
 Task AddAsync(T entity, CancellationToken cancellationToken = default);
 void Update(T entity);
 void Remove(T entity);
}
