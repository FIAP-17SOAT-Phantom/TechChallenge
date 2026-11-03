using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Infrastructure.Notifications;

namespace OficinaMecanica.Worker;

public sealed class OutboxBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxBackgroundService> _logger;
    private readonly TimeSpan _pollingInterval;

    public OutboxBackgroundService(IServiceScopeFactory scopeFactory, IOptions<OutboxOptions> options, ILogger<OutboxBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _pollingInterval = TimeSpan.FromSeconds(Math.Max(1, options.Value.PollingIntervalSeconds));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker de notificacoes iniciado. Intervalo: {PollingInterval}", _pollingInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessBatchAsync(stoppingToken);
            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
            var processed = await processor.ProcessPendingAsync(cancellationToken);

            if (processed > 0)
            {
                _logger.LogInformation("Lote Outbox processado. Mensagens: {MessageCount}", processed);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Falha no ciclo de processamento do Outbox");
        }
    }
}
