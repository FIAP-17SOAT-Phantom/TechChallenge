using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Estoque.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Repositories;

public sealed class AlertaEstoqueRepository : IAlertaEstoqueRepository
{
    private readonly AppDbContext _context;

    public AlertaEstoqueRepository(AppDbContext context) => _context = context;

    public async Task<AlertaEstoque?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => await _context.AlertasEstoque.FirstOrDefaultAsync(alerta => alerta.Id == id, cancellationToken);

    public async Task<AlertaEstoque?> GetAtivoByPecaIdAsync(Guid pecaId, CancellationToken cancellationToken = default) => await _context.AlertasEstoque.FirstOrDefaultAsync(alerta => alerta.PecaId == pecaId && alerta.DataResolucao == null, cancellationToken);

    public async Task<IReadOnlyList<AlertaEstoque>> GetAllAsync(bool somenteAtivos, int page, int pageSize, CancellationToken cancellationToken = default) => await _context.AlertasEstoque.Where(alerta => !somenteAtivos || alerta.DataResolucao == null).OrderByDescending(alerta => alerta.DataCriacao).Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(AlertaEstoque entity, CancellationToken cancellationToken = default) => await _context.AlertasEstoque.AddAsync(entity, cancellationToken);

    public void Update(AlertaEstoque entity) => _context.AlertasEstoque.Update(entity);

    public void Remove(AlertaEstoque entity) => _context.AlertasEstoque.Remove(entity);
}
