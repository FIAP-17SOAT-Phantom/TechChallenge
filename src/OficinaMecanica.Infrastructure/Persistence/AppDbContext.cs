using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Atendimento.Entities;
using OficinaMecanica.Domain.CatalogoServicos.Entities;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Estoque.Entities;
using OficinaMecanica.Domain.Oficina.Entities;
using OficinaMecanica.Domain.Orcamentacao.Entities;

namespace OficinaMecanica.Infrastructure.Persistence;

/// <summary>
/// DbContext da aplicacao.
/// Implementa IUnitOfWork para garantir atomicidade por use case.
/// Despacha Domain Events APOS o SaveChanges para manter
/// a consistencia entre aggregates via MediatR notifications.
/// </summary>
public class AppDbContext : DbContext, IUnitOfWork
{
 private readonly IMediator _mediator;
 private readonly ILogger<AppDbContext> _logger;

 // DbSets apenas para Aggregate Roots
 // Reserva NAO tem DbSet - e acessada apenas via Peca aggregate
 public DbSet<Cliente> Clientes => Set<Cliente>();
 public DbSet<Veiculo> Veiculos => Set<Veiculo>();
 public DbSet<OrdemDeServico> OrdensDeServico => Set<OrdemDeServico>();
 public DbSet<Orcamento> Orcamentos => Set<Orcamento>();
 public DbSet<Peca> Pecas => Set<Peca>();
 public DbSet<Servico> Servicos => Set<Servico>();

 public AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator, ILogger<AppDbContext> logger)
 : base(options)
 {
 _mediator = mediator;
 _logger = logger;
 }

 protected override void OnModelCreating(ModelBuilder modelBuilder)
 {
 modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
 base.OnModelCreating(modelBuilder);
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
 .Where(e => e.Entity.DomainEvents.Any())
 .Select(e => e.Entity)
 .ToList();

 var domainEvents = aggregatesWithEvents
 .SelectMany(a => a.DomainEvents)
 .ToList();

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
}
