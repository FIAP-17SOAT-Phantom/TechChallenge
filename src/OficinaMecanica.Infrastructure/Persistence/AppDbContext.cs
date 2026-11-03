using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Atendimento.Entities;
using OficinaMecanica.Domain.CatalogoServicos.Entities;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Estoque.Entities;
using OficinaMecanica.Domain.Oficina.Entities;
using OficinaMecanica.Domain.Oficina.Events;
using OficinaMecanica.Domain.Orcamentacao.Entities;
using OficinaMecanica.Domain.Orcamentacao.Events;
using OficinaMecanica.Infrastructure.Identity;
using OficinaMecanica.Infrastructure.Notifications;

namespace OficinaMecanica.Infrastructure.Persistence;

/// <summary>
/// DbContext da aplicacao.
/// Implementa IUnitOfWork para garantir atomicidade por use case.
/// Despacha Domain Events APOS o SaveChanges para manter
/// a consistencia entre aggregates via MediatR notifications.
/// </summary>
public class AppDbContext : IdentityDbContext<UsuarioSistema>, IUnitOfWork
{
    private readonly IMediator _mediator;
    private readonly ILogger<AppDbContext> _logger;

    // DbSets apenas para Aggregate Roots
    // Reserva NAO tem DbSet - e acessada apenas via Peca aggregate
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<OrdemDeServico> OrdensDeServico => Set<OrdemDeServico>();
    public DbSet<Orcamento> Orcamentos => Set<Orcamento>();
    public DbSet<EventoRespostaOrcamento> EventosRespostaOrcamento => Set<EventoRespostaOrcamento>();
    public DbSet<Peca> Pecas => Set<Peca>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<AlertaEstoque> AlertasEstoque => Set<AlertaEstoque>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator, ILogger<AppDbContext> logger)
    : base(options)
    {
        _mediator = mediator;
        _logger = logger;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasSequence<long>("ordem_servico_numero_seq").StartsAt(1).IncrementsBy(1);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>
    /// Salva mudancas e despacha Domain Events.
    /// Eventos sao despachados APOS o save para garantir que os dados
    /// ja estao persistidos quando os handlers executam.
    /// Se um handler falhar, o evento e logado (outbox pattern seria o fix ideal).
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Coletar domain events antes do save
        var aggregatesWithEvents = ChangeTracker.Entries<AggregateRoot>()
        .Where(e => e.Entity.DomainEvents.Count > 0)
        .Select(e => e.Entity)
        .ToList();

        var domainEvents = aggregatesWithEvents
        .SelectMany(a => a.DomainEvents)
        .ToList();

        var outboxMessages = domainEvents.Where(ShouldPersistInOutbox).Where(domainEvent => !OutboxMessages.Local.Any(message => message.Id == domainEvent.EventId)).Select(OutboxMessage.FromDomainEvent).ToList();

        if (outboxMessages.Count > 0)
        {
            await OutboxMessages.AddRangeAsync(outboxMessages, cancellationToken);
        }

        // Persistir primeiro
        var result = await base.SaveChangesAsync(cancellationToken);

        // Limpar eventos APOS save bem-sucedido
        aggregatesWithEvents.ForEach(a => a.ClearDomainEvents());

        // Despachar eventos (dados ja persistidos)
        foreach (var domainEvent in domainEvents)
        {
            try
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                "Falha ao despachar domain event {EventType}. Evento perdido: {@Event}",
                domainEvent.GetType().Name, domainEvent);
            }
        }

        return result;
    }

    private static bool ShouldPersistInOutbox(IDomainEvent domainEvent) => domainEvent is OrdemDeServicoStatusAlteradoEvent or OrcamentoEnviadoEvent or OrcamentoAprovadoEvent or OrcamentoRejeitadoEvent;

    public Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default) => ExecuteTransactionAsync(operation, IsolationLevel.Serializable, cancellationToken);

    public Task<T> ExecuteInReadCommittedTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default) => ExecuteTransactionAsync(operation, IsolationLevel.ReadCommitted, cancellationToken);

    private async Task<T> ExecuteTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, IsolationLevel isolationLevel, CancellationToken cancellationToken)
    {
        await using var transaction = await Database.BeginTransactionAsync(isolationLevel, cancellationToken);

        try
        {
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
