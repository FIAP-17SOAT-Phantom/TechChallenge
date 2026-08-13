using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Oficina.Entities;
using OficinaMecanica.Domain.Oficina.Enums;

namespace OficinaMecanica.Infrastructure.Persistence.Repositories;

public class OrdemDeServicoRepository : IOrdemDeServicoRepository
{
 private readonly AppDbContext _context;

 public OrdemDeServicoRepository(AppDbContext context) => _context = context;

 public async Task<OrdemDeServico?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
 => await _context.OrdensDeServico
 .Include(os => os.Itens)
 .FirstOrDefaultAsync(os => os.Id == id, cancellationToken);

 public async Task<OrdemDeServico?> GetByNumeroAsync(string numero, CancellationToken cancellationToken = default)
 => await _context.OrdensDeServico
 .Include(os => os.Itens)
 .FirstOrDefaultAsync(os => os.Numero == numero, cancellationToken);

 public async Task<IReadOnlyList<OrdemDeServico>> GetByClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
 => await _context.OrdensDeServico
 .Where(os => os.ClienteId == clienteId)
 .AsNoTracking()
 .OrderByDescending(os => os.DataAbertura)
 .ToListAsync(cancellationToken);

 public async Task<IReadOnlyList<OrdemDeServico>> GetByStatusAsync(StatusOS status, CancellationToken cancellationToken = default)
 => await _context.OrdensDeServico
 .Where(os => os.Status == status)
 .AsNoTracking()
 .OrderByDescending(os => os.DataAbertura)
 .ToListAsync(cancellationToken);

 public async Task<IReadOnlyList<OrdemDeServico>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
 => await _context.OrdensDeServico
 .AsNoTracking()
 .OrderByDescending(os => os.DataAbertura)
 .Skip((page - 1) * pageSize)
 .Take(pageSize)
 .ToListAsync(cancellationToken);

 /// <summary>
 /// Gera proximo numero sequencial para OS.
 /// Usa COUNT + 1 ao inves de string parsing para evitar
 /// problemas com ordenacao lexicografica.
 /// O unique index no campo Numero protege contra duplicatas em race conditions.
 /// Em producao, usar uma sequence do PostgreSQL seria mais robusto.
 /// </summary>
 public async Task<string> GerarProximoNumeroAsync(CancellationToken cancellationToken = default)
 {
 var count = await _context.OrdensDeServico.CountAsync(cancellationToken);
 return $"OS-{count + 1:D4}";
 }

 public async Task AddAsync(OrdemDeServico entity, CancellationToken cancellationToken = default)
 => await _context.OrdensDeServico.AddAsync(entity, cancellationToken);

 public void Update(OrdemDeServico entity) => _context.OrdensDeServico.Update(entity);

 public void Remove(OrdemDeServico entity) => _context.OrdensDeServico.Remove(entity);
}
