using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Infrastructure.Persistence;
using OficinaMecanica.Infrastructure.Persistence.Repositories;

namespace OficinaMecanica.Infrastructure;

public static class DependencyInjection
{
 public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
 {
 // DbContext + PostgreSQL
 services.AddDbContext<AppDbContext>(options =>
 options.UseNpgsql(
 configuration.GetConnectionString("DefaultConnection"),
 b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

 // Unit of Work (o proprio DbContext)
 services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());

 // Repositories
 services.AddScoped<IClienteRepository, ClienteRepository>();
 services.AddScoped<IVeiculoRepository, VeiculoRepository>();
 services.AddScoped<IOrdemDeServicoRepository, OrdemDeServicoRepository>();
 services.AddScoped<IOrcamentoRepository, OrcamentoRepository>();
 services.AddScoped<IPecaRepository, PecaRepository>();
 services.AddScoped<IServicoRepository, ServicoRepository>();

 return services;
 }
}
