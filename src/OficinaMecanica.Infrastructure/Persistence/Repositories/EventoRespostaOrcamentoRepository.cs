using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Orcamentacao.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Repositories;

public sealed class EventoRespostaOrcamentoRepository : IEventoRespostaOrcamentoRepository
{
    private readonly AppDbContext _context;

    public EventoRespostaOrcamentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AcquireOrcamentoLockAsync(Guid orcamentoId, CancellationToken cancellationToken = default)
    {
        var lockKey = BitConverter.ToInt64(orcamentoId.ToByteArray(), 0);
        await _context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({lockKey})", cancellationToken);
    }

    public async Task<EventoRespostaOrcamento?> GetByExternalEventIdAsync(string externalEventId, CancellationToken cancellationToken = default) => await _context.EventosRespostaOrcamento.AsNoTracking().SingleOrDefaultAsync(evento => evento.ExternalEventId == externalEventId, cancellationToken);

    public async Task<bool> TryAddAsync(EventoRespostaOrcamento evento, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync($$"""
            INSERT INTO "EventosRespostaOrcamento" ("Id", "ExternalEventId", "OrcamentoId", "Resposta", "TimestampUnix", "RecebidoEmUtc", "ProcessadoEmUtc", "ProcessadoComSucesso", "TipoErro", "Erro")
            VALUES ({{evento.Id}}, {{evento.ExternalEventId}}, {{evento.OrcamentoId}}, {{evento.Resposta.ToString()}}, {{evento.TimestampUnix}}, {{evento.RecebidoEmUtc}}, {{evento.ProcessadoEmUtc}}, {{evento.ProcessadoComSucesso}}, {{(evento.TipoErro.HasValue ? evento.TipoErro.Value.ToString() : null)}}, {{evento.Erro}})
            ON CONFLICT ("ExternalEventId") DO NOTHING
            """, cancellationToken);

        return affectedRows == 1;
    }
}
