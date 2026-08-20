using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.API.Extensions;
using OficinaMecanica.Application.UseCases.Estoque.Commands;
using OficinaMecanica.Application.UseCases.Estoque.Queries;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/alertas-estoque")]
public sealed class AlertasEstoqueController : ControllerBase
{
    private readonly IMediator _mediator;

    public AlertasEstoqueController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool somenteAtivos = true, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken cancellationToken = default)
    {
        var alertas = await _mediator.Send(new ListarAlertasEstoqueQuery(somenteAtivos, pagina, tamanhoPagina), cancellationToken);

        return Ok(alertas);
    }

    [HttpPatch("{alertaId:guid}/visualizar")]
    public async Task<IActionResult> Visualizar(Guid alertaId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AlterarStatusAlertaEstoqueCommand(alertaId, false), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }

    [HttpPatch("{alertaId:guid}/resolver")]
    public async Task<IActionResult> Resolver(Guid alertaId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AlterarStatusAlertaEstoqueCommand(alertaId, true), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }
}
