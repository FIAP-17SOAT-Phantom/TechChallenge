using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Atendimento.Entities;
using OficinaMecanica.Domain.Atendimento.ValueObjects;

namespace OficinaMecanica.Infrastructure.Persistence.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context) => _context = context;

    public async Task<Cliente?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    => await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Cliente?> GetByCpfAsync(Cpf cpf, CancellationToken cancellationToken = default)
    => await _context.Clientes.FirstOrDefaultAsync(c => c.Cpf.Numero == cpf.Numero, cancellationToken);

    public async Task<IReadOnlyList<Cliente>> GetAllAsync(CancellationToken cancellationToken = default)
    => await _context.Clientes.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Cliente>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default) => await _context.Clientes.AsNoTracking().OrderBy(cliente => cliente.Nome).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

    public async Task<bool> ExistsByCpfAsync(Cpf cpf, CancellationToken cancellationToken = default)
    => await _context.Clientes.AnyAsync(c => c.Cpf.Numero == cpf.Numero, cancellationToken);

    public async Task AddAsync(Cliente entity, CancellationToken cancellationToken = default)
    => await _context.Clientes.AddAsync(entity, cancellationToken);

    public void Update(Cliente entity) => _context.Clientes.Update(entity);

    public void Remove(Cliente entity) => _context.Clientes.Remove(entity);
}
