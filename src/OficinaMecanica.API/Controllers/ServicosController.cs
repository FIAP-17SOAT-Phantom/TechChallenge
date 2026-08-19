using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.UseCases.CatalogoServicos.Commands;
using OficinaMecanica.Application.UseCases.CatalogoServicos.Queries;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/servicos")]
public sealed class ServicosController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServicosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarServicoCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { erro = result.Error });
        }

        return CreatedAtAction(nameof(Consultar), new { servicoId = result.Value }, new { id = result.Value });
    }

    [HttpGet("{servicoId:guid}")]
    public async Task<IActionResult> Consultar(Guid servicoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConsultarServicoQuery(servicoId), cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { erro = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool somenteAtivos = true, CancellationToken cancellationToken = default)
    {
        var servicos = await _mediator.Send(new ListarServicosQuery(somenteAtivos), cancellationToken);

        return Ok(servicos);
    }

    [HttpPut("{servicoId:guid}")]
    public async Task<IActionResult> Atualizar(Guid servicoId, AtualizarServicoRequest request, CancellationToken cancellationToken)
    {
        var command = new AtualizarServicoCommand(servicoId, request.Nome, request.Descricao, request.PrecoBase, request.TempoEstimadoMinutos);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { erro = result.Error });
        }

        return NoContent();
    }

    [HttpDelete("{servicoId:guid}")]
    public async Task<IActionResult> Desativar(Guid servicoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AlterarStatusServicoCommand(servicoId, false), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { erro = result.Error });
        }

        return NoContent();
    }

    [HttpPatch("{servicoId:guid}/ativar")]
    public async Task<IActionResult> Ativar(Guid servicoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AlterarStatusServicoCommand(servicoId, true), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { erro = result.Error });
        }

        return NoContent();
    }
}

public sealed record AtualizarServicoRequest(string Nome, string Descricao, decimal PrecoBase, int TempoEstimadoMinutos);
