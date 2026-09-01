using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.Common.Interfaces;
using System.Security.Claims;

namespace OficinaMecanica.API.Authentication;

public sealed class UserAccessMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext, IIdentityService identityService, IProblemDetailsService problemDetailsService)
    {
        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            await next(httpContext);
            return;
        }

        var usuarioId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var estadoAcesso = string.IsNullOrWhiteSpace(usuarioId) ? null : await identityService.ObterEstadoAcessoAsync(usuarioId, httpContext.RequestAborted);

        if (estadoAcesso is null || !estadoAcesso.Ativo)
        {
            await WriteProblemAsync(httpContext, problemDetailsService, StatusCodes.Status401Unauthorized, "Usuario inativo", "O acesso deste usuario esta desativado.");
            return;
        }

        var rotaAlteracaoSenha = httpContext.Request.Path.Equals("/api/auth/alterar-senha", StringComparison.OrdinalIgnoreCase);

        if (estadoAcesso.TrocaSenhaObrigatoria && !rotaAlteracaoSenha)
        {
            await WriteProblemAsync(httpContext, problemDetailsService, StatusCodes.Status403Forbidden, "Troca de senha obrigatoria", "Altere a senha temporaria antes de acessar os demais recursos.");
            return;
        }

        await next(httpContext);
    }

    private static async Task WriteProblemAsync(HttpContext httpContext, IProblemDetailsService problemDetailsService, int status, string title, string detail)
    {
        httpContext.Response.StatusCode = status;
        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = detail, Instance = httpContext.Request.Path } });
    }
}
