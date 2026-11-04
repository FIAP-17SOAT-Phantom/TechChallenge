using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OficinaMecanica.Application;
using OficinaMecanica.API.ExceptionHandling;
using OficinaMecanica.Infrastructure;
using OficinaMecanica.Infrastructure.Identity;
using OficinaMecanica.API.OpenApi;
using OficinaMecanica.API.Configuration;
using OficinaMecanica.API.Authentication;
using OficinaMecanica.API.Health;
using System.Text;

DotEnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("postgresql", tags: ["ready"]);
builder.Services.Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromSeconds(30));

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer nao configurado");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience nao configurado");
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret nao configurado");

if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
{
    throw new InvalidOperationException("Jwt:Secret deve possuir pelo menos 32 bytes");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Oficina Mecanica API", Version = "v1", Description = "API do Sistema Integrado de Atendimento e Execucao de Servicos da Oficina Mecanica" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", In = ParameterLocation.Header, Description = "Informe o token JWT obtido em /api/auth/login" });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>() });
    options.OperationFilter<ApiDocumentationOperationFilter>();
    options.OrderActionsBy(apiDescription => $"{apiDescription.ActionDescriptor.RouteValues["controller"]}_{apiDescription.RelativePath}");
});

var app = builder.Build();

// Swagger (apenas em Desenvolvimento). A API usa uma FallbackPolicy global que
// exige usuario autenticado em todo request sem [AllowAnonymous]. A UI do
// Swashbuckle e servida por middleware (nao por um endpoint com metadata de
// autorizacao), entao cai no fallback e retorna 401.
//
// MapWhen cria um pipeline COMPLETAMENTE separado para "/swagger": requisicoes
// desse caminho sao atendidas so pelo Swagger e nunca chegam ao UseRouting /
// UseAuthorization do pipeline principal (onde a FallbackPolicy e aplicada).
// Por isso usamos MapWhen (ramo terminal) em vez de UseWhen (ramo que retorna
// ao pipeline principal).
if (app.Environment.IsDevelopment())
{
    app.MapWhen(
        context => context.Request.Path.StartsWithSegments("/swagger"),
        branch =>
        {
            branch.UseSwagger();
            branch.UseSwaggerUI();
        });
}

app.UseHttpsRedirection();

app.UseExceptionHandler();
app.UseStatusCodePages(async statusCodeContext =>
{
    var problemDetailsService = statusCodeContext.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
    var status = statusCodeContext.HttpContext.Response.StatusCode;
    var title = status switch
    {
        StatusCodes.Status401Unauthorized => "Nao autenticado",
        StatusCodes.Status403Forbidden => "Acesso negado",
        StatusCodes.Status404NotFound => "Recurso nao encontrado",
        _ => "Falha ao processar a requisicao"
    };

    await problemDetailsService.TryWriteAsync(new ProblemDetailsContext { HttpContext = statusCodeContext.HttpContext, ProblemDetails = new ProblemDetails { Status = status, Title = title, Instance = statusCodeContext.HttpContext.Request.Path } });
});

app.UseAuthentication();
app.UseMiddleware<UserAccessMiddleware>();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = registration => registration.Tags.Contains("ready") }).AllowAnonymous();

var migrateOnly = builder.Configuration.GetValue<bool>("Database:MigrateOnly");
var applyMigrationsOnStartup = builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");
var seedOnStartup = builder.Configuration.GetValue<bool>("Authentication:SeedOnStartup");

if (migrateOnly || applyMigrationsOnStartup)
{
    await app.Services.MigrateDatabaseAsync();
}

if (seedOnStartup)
{
    await app.Services.SeedIdentityAsync();
}

if (migrateOnly)
{
    return;
}

await app.RunAsync();

// Expoe a classe Program para testes de integracao (WebApplicationFactory)
public partial class Program { protected Program() { } }
