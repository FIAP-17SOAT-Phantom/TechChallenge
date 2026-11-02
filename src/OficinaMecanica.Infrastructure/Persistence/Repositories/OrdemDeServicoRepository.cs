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
    .Include(os => os.ServicosSolicitados)
    .Include(os => os.PecasSolicitadas)
    .AsSplitQuery()
    .FirstOrDefaultAsync(os => os.Id == id, cancellationToken);

    public async Task<OrdemDeServico?> GetByNumeroAsync(string numero, CancellationToken cancellationToken = default)
    => await _context.OrdensDeServico
    .Include(os => os.Itens)
    .Include(os => os.ServicosSolicitados)
    .Include(os => os.PecasSolicitadas)
    .AsSplitQuery()
    .FirstOrDefaultAsync(os => os.Numero == numero, cancellationToken);

    public async Task<IReadOnlyList<OrdemDeServico>> GetByClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
    => await _context.OrdensDeServico
    .Include(os => os.Itens)
    .Include(os => os.ServicosSolicitados)
    .Include(os => os.PecasSolicitadas)
    .Where(os => os.ClienteId == clienteId)
    .AsNoTracking()
    .AsSplitQuery()
    .OrderByDescending(os => os.DataAbertura)
    .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<OrdemDeServico>> GetPagedByClienteIdAsync(Guid clienteId, StatusOS? status, int page, int pageSize, CancellationToken cancellationToken = default) => await _context.OrdensDeServico.Include(os => os.Itens).Include(os => os.ServicosSolicitados).Include(os => os.PecasSolicitadas).Where(os => os.ClienteId == clienteId && (!status.HasValue || os.Status == status.Value)).AsNoTracking().AsSplitQuery().OrderByDescending(os => os.DataAbertura).ThenBy(os => os.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<OrdemDeServico>> GetByStatusAsync(StatusOS status, CancellationToken cancellationToken = default)
    => await _context.OrdensDeServico
    .Include(os => os.Itens)
    .Include(os => os.ServicosSolicitados)
    .Include(os => os.PecasSolicitadas)
    .Where(os => os.Status == status)
    .AsNoTracking()
    .AsSplitQuery()
    .OrderByDescending(os => os.DataAbertura)
    .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<OrdemDeServico>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    => await _context.OrdensDeServico
    .Include(os => os.Itens)
    .Include(os => os.ServicosSolicitados)
    .Include(os => os.PecasSolicitadas)
    .AsNoTracking()
    .AsSplitQuery()
    .OrderByDescending(os => os.DataAbertura)
    .ThenBy(os => os.Id)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
        .ToListAsync(cancellationToken);

    public async Task<bool> ExistsByVeiculoIdAsync(Guid veiculoId, CancellationToken cancellationToken = default)
    => await _context.OrdensDeServico.AnyAsync(os => os.VeiculoId == veiculoId, cancellationToken);

    public async Task<TimeSpan?> GetTempoMedioExecucaoAsync(CancellationToken cancellationToken = default)
    {
        var periodos = await _context.OrdensDeServico.Where(os => os.DataInicioExecucao.HasValue && os.DataFinalizacao.HasValue).Select(os => new { Inicio = os.DataInicioExecucao!.Value, Fim = os.DataFinalizacao!.Value }).ToListAsync(cancellationToken);

        return periodos.Count == 0 ? null : TimeSpan.FromSeconds(periodos.Average(periodo => (periodo.Fim - periodo.Inicio).TotalSeconds));
    }

    /// <summary>
    /// Gera o proximo numero da OS por uma sequence atomica do PostgreSQL.
    /// A sequence evita colisoes quando varias instancias da API abrem ordens simultaneamente.
    /// </summary>
    public async Task<string> GerarProximoNumeroAsync(CancellationToken cancellationToken = default)
    {
        var sequencial = await _context.Database.SqlQueryRaw<long>("SELECT nextval('ordem_servico_numero_seq') AS \"Value\"").SingleAsync(cancellationToken);
        return $"OS-{sequencial:D4}";
    }

    public async Task AddAsync(OrdemDeServico entity, CancellationToken cancellationToken = default)
    => await _context.OrdensDeServico.AddAsync(entity, cancellationToken);

    public void Update(OrdemDeServico entity) => _context.OrdensDeServico.Update(entity);

    public void Remove(OrdemDeServico entity) => _context.OrdensDeServico.Remove(entity);
}
