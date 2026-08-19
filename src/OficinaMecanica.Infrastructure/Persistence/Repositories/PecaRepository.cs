using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Estoque.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Repositories;

public class PecaRepository : IPecaRepository
{
    private readonly AppDbContext _context;

    public PecaRepository(AppDbContext context) => _context = context;

    public async Task<Peca?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    => await _context.Pecas
    .Include(p => p.Reservas)
    .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Peca?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    => await _context.Pecas
    .Include(p => p.Reservas)
    .FirstOrDefaultAsync(p => p.Codigo == codigo, cancellationToken);

    public async Task<IReadOnlyList<Peca>> GetAllAsync(CancellationToken cancellationToken = default)
    => await _context.Pecas.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Peca>> GetComEstoqueBaixoAsync(CancellationToken cancellationToken = default)
    => await _context.Pecas
    .Where(p => p.QuantidadeEmEstoque - p.QuantidadeReservada <= p.QuantidadeMinima)
    .AsNoTracking()
    .ToListAsync(cancellationToken);

    public async Task AddAsync(Peca entity, CancellationToken cancellationToken = default)
    => await _context.Pecas.AddAsync(entity, cancellationToken);

    public void Update(Peca entity) => _context.Pecas.Update(entity);

    public void Remove(Peca entity) => _context.Pecas.Remove(entity);
}
