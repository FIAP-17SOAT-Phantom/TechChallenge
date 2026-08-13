using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.CatalogoServicos.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Repositories;

public class ServicoRepository : IServicoRepository
{
 private readonly AppDbContext _context;

 public ServicoRepository(AppDbContext context) => _context = context;

 public async Task<Servico?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
 => await _context.Servicos.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

 public async Task<IReadOnlyList<Servico>> GetAllAtivosAsync(CancellationToken cancellationToken = default)
 => await _context.Servicos.Where(s => s.Ativo).AsNoTracking().ToListAsync(cancellationToken);

 public async Task<IReadOnlyList<Servico>> GetAllAsync(CancellationToken cancellationToken = default)
 => await _context.Servicos.AsNoTracking().ToListAsync(cancellationToken);

 public async Task AddAsync(Servico entity, CancellationToken cancellationToken = default)
 => await _context.Servicos.AddAsync(entity, cancellationToken);

 public void Update(Servico entity) => _context.Servicos.Update(entity);

 public void Remove(Servico entity) => _context.Servicos.Remove(entity);
}
