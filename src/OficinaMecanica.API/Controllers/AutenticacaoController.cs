using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.UseCases.Seguranca.Commands;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AutenticacaoController : ControllerBase
{
    private readonly IMediator _mediator;

    public AutenticacaoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(AutenticarCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(new ProblemDetails { Status = StatusCodes.Status401Unauthorized, Title = "Credenciais invalidas", Detail = result.Error });
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("usuarios")]
    public async Task<IActionResult> CriarUsuario(CriarUsuarioCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Nao foi possivel criar o usuario", Detail = result.Error });
        }

        return Created(string.Empty, new { id = result.Value });
    }
}
