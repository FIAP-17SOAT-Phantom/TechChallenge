using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Orcamentacao.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Repositories;

public class OrcamentoRepository : IOrcamentoRepository
{
    private readonly AppDbContext _context;

    public OrcamentoRepository(AppDbContext context) => _context = context;

    public async Task<Orcamento?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    => await _context.Orcamentos
    .Include(o => o.Itens)
    .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<Orcamento?> GetByOrdemDeServicoIdAsync(Guid ordemDeServicoId, CancellationToken cancellationToken = default)
    => await _context.Orcamentos
    .Include(o => o.Itens)
    .Where(o => o.OrdemDeServicoId == ordemDeServicoId)
    .OrderByDescending(o => o.Versao)
    .FirstOrDefaultAsync(cancellationToken);

    public async Task<int> GetVersaoAtualAsync(Guid ordemDeServicoId, CancellationToken cancellationToken = default)
    {
        var maxVersao = await _context.Orcamentos
        .Where(o => o.OrdemDeServicoId == ordemDeServicoId)
        .MaxAsync(o => (int?)o.Versao, cancellationToken);

        return maxVersao ?? 0;
    }

    public async Task AddAsync(Orcamento entity, CancellationToken cancellationToken = default)
    => await _context.Orcamentos.AddAsync(entity, cancellationToken);

    public void Update(Orcamento entity) => _context.Orcamentos.Update(entity);

    public void Remove(Orcamento entity) => _context.Orcamentos.Remove(entity);
}
