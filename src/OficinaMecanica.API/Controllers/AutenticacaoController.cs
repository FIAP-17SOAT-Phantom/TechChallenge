using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.UseCases.Seguranca.Commands;
using OficinaMecanica.Application.UseCases.Seguranca.Queries;
using OficinaMecanica.API.Extensions;
using System.Security.Claims;
using System.Text.Json.Serialization;

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
            return this.ToProblem(result);
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
            return this.ToProblem(result);
        }

        return Created(string.Empty, result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("usuarios")]
    public async Task<IActionResult> ListarUsuarios([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken cancellationToken = default)
    {
        var usuarios = await _mediator.Send(new ListarUsuariosQuery(pagina, tamanhoPagina), cancellationToken);

        return Ok(usuarios);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("usuarios/{usuarioId}")]
    public async Task<IActionResult> ConsultarUsuario(string usuarioId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConsultarUsuarioQuery(usuarioId), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("usuarios/{usuarioId}/status")]
    public async Task<IActionResult> AlterarStatusUsuario(string usuarioId, AlterarStatusUsuarioRequest request, CancellationToken cancellationToken)
    {
        var usuarioSolicitanteId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new AlterarStatusUsuarioCommand(usuarioId, request.Ativo, usuarioSolicitanteId), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("usuarios/{usuarioId}/redefinir-senha")]
    public async Task<IActionResult> RedefinirSenhaUsuario(string usuarioId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RedefinirSenhaUsuarioCommand(usuarioId), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpPost("alterar-senha")]
    public async Task<IActionResult> AlterarSenha(AlterarSenhaRequest request, CancellationToken cancellationToken)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(usuarioId))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new AlterarSenhaCommand(usuarioId, request.SenhaAtual, request.NovaSenha), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }
}

public sealed record AlterarSenhaRequest(string SenhaAtual, string NovaSenha);
public sealed record AlterarStatusUsuarioRequest([property: JsonRequired] bool Ativo);
