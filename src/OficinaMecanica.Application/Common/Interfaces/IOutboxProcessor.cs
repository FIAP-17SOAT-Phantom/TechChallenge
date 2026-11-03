namespace OficinaMecanica.Application.Common.Interfaces;

public interface IOutboxProcessor
{
    Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default);
}
