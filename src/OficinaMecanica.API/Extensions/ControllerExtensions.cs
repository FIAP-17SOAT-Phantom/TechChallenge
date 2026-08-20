using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.API.Extensions;

public static class ControllerExtensions
{
    public static ObjectResult ToProblem(this ControllerBase controller, Result result)
    {
        var (status, title) = result.ErrorType switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Erro de validacao"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Recurso nao encontrado"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflito ao processar o recurso"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Credenciais invalidas"),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Acesso negado"),
            _ => (StatusCodes.Status400BadRequest, "Regra de negocio nao atendida")
        };

        return controller.Problem(detail: result.Error, statusCode: status, title: title);
    }

}
