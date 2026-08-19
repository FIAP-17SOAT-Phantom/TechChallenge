using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OficinaMecanica.API.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors.GroupBy(error => error.PropertyName).ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
            var problemDetails = new ValidationProblemDetails(errors) { Status = StatusCodes.Status400BadRequest, Title = "Erro de validacao" };
            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = problemDetails });
        }

        var (status, title) = exception switch
        {
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflito de concorrencia"),
            DbUpdateException => (StatusCodes.Status409Conflict, "Conflito de persistencia"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Nao autorizado"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Requisicao invalida"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Operacao invalida"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Erro inesperado ao processar {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }

        var details = status == StatusCodes.Status500InternalServerError ? "Ocorreu um erro inesperado." : exception.Message;
        var response = new ProblemDetails { Status = status, Title = title, Detail = details, Instance = httpContext.Request.Path };
        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = response });
    }
}
