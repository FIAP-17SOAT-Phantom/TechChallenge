namespace OficinaMecanica.Application.Common.Interfaces;

/// <summary>
/// Garante atomicidade nas operacoes de persistencia.
/// Uma transacao por use case.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default);
}
