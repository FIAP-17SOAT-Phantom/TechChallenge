using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Atendimento.Entities;
using OficinaMecanica.Domain.Atendimento.ValueObjects;

namespace OficinaMecanica.Infrastructure.Persistence.Repositories;

public class VeiculoRepository : IVeiculoRepository
{
    private readonly AppDbContext _context;

    public VeiculoRepository(AppDbContext context) => _context = context;

    public async Task<Veiculo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    => await _context.Veiculos.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task<Veiculo?> GetByPlacaAsync(Placa placa, CancellationToken cancellationToken = default)
    => await _context.Veiculos.FirstOrDefaultAsync(v => v.Placa.Valor == placa.Valor, cancellationToken);

    public async Task<IReadOnlyList<Veiculo>> GetByClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
    => await _context.Veiculos.Where(v => v.ClienteId == clienteId).AsNoTracking().ToListAsync(cancellationToken);

    public async Task<bool> ExistsByPlacaAsync(Placa placa, CancellationToken cancellationToken = default)
    => await _context.Veiculos.AnyAsync(v => v.Placa.Valor == placa.Valor, cancellationToken);

    public async Task AddAsync(Veiculo entity, CancellationToken cancellationToken = default)
    => await _context.Veiculos.AddAsync(entity, cancellationToken);

    public void Update(Veiculo entity) => _context.Veiculos.Update(entity);

    public void Remove(Veiculo entity) => _context.Veiculos.Remove(entity);
}
