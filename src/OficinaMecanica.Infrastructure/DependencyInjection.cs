using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Infrastructure.Identity;
using OficinaMecanica.Infrastructure.Notifications;
using OficinaMecanica.Infrastructure.Integracoes;
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
                builder => builder.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Unit of Work (o proprio DbContext)
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());

        // Repositories
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IVeiculoRepository, VeiculoRepository>();
        services.AddScoped<IOrdemDeServicoRepository, OrdemDeServicoRepository>();
        services.AddScoped<IOrcamentoRepository, OrcamentoRepository>();
        services.AddScoped<IPecaRepository, PecaRepository>();
        services.AddScoped<IServicoRepository, ServicoRepository>();
        services.AddScoped<IAlertaEstoqueRepository, AlertaEstoqueRepository>();
        services.AddScoped<IEventoRespostaOrcamentoRepository, EventoRespostaOrcamentoRepository>();
        services.AddScoped<IWebhookOrcamentoAuthenticator, HmacWebhookOrcamentoAuthenticator>();

        services.AddIdentityCore<UsuarioSistema>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>();

        services.Configure<JwtOptions>(options =>
        {
            options.Issuer = configuration["Jwt:Issuer"] ?? string.Empty;
            options.Audience = configuration["Jwt:Audience"] ?? string.Empty;
            options.Secret = configuration["Jwt:Secret"] ?? string.Empty;
            options.ExpirationMinutes = int.TryParse(configuration["Jwt:ExpirationMinutes"], out var expirationMinutes) ? expirationMinutes : 60;
        });
        services.AddScoped<IIdentityService, IdentityService>();

        services.Configure<OutboxOptions>(options =>
        {
            options.PollingIntervalSeconds = ParsePositiveInt(configuration["Outbox:PollingIntervalSeconds"], 5);
            options.BatchSize = ParsePositiveInt(configuration["Outbox:BatchSize"], 20);
            options.MaxAttempts = ParsePositiveInt(configuration["Outbox:MaxAttempts"], 5);
            options.LockSeconds = ParsePositiveInt(configuration["Outbox:LockSeconds"], 120);
        });
        services.Configure<EmailOptions>(options =>
        {
            options.Provider = configuration["Email:Provider"] ?? "Log";
            options.From = configuration["Email:From"] ?? "Oficina Mecanica <nao-responda@exemplo.com>";
            options.ResendApiKey = configuration["Email:ResendApiKey"];
        });
        services.Configure<WebhookOrcamentoOptions>(options =>
        {
            options.Secret = configuration["Webhook:OrcamentoSecret"] ?? string.Empty;
            options.ToleranceSeconds = ParsePositiveInt(configuration["Webhook:ToleranceSeconds"], 300);
        });
        services.AddScoped<IOutboxProcessor, OutboxProcessor>();

        var emailProvider = configuration["Email:Provider"] ?? "Log";
        var emailFrom = configuration["Email:From"];

        if (!string.Equals(emailProvider, "Log", StringComparison.OrdinalIgnoreCase) && !string.Equals(emailProvider, "Resend", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Email:Provider deve ser Log ou Resend");
        }

        if (string.IsNullOrWhiteSpace(emailFrom))
        {
            throw new InvalidOperationException("Email:From nao configurado");
        }

        if (string.Equals(emailProvider, "Resend", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(configuration["Email:ResendApiKey"]))
            {
                throw new InvalidOperationException("Email:ResendApiKey nao configurado para o provider Resend");
            }

            services.AddSingleton(new HttpClient { BaseAddress = new Uri("https://api.resend.com/"), Timeout = TimeSpan.FromSeconds(15) });
            services.AddScoped<IEmailSender, ResendEmailSender>();
        }
        else
        {
            services.AddScoped<IEmailSender, LoggingEmailSender>();
        }

        return services;
    }

    private static int ParsePositiveInt(string? value, int fallback) => int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
}
