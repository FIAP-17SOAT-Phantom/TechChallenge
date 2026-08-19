using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.UseCases.Atendimento.Commands;
using OficinaMecanica.Application.UseCases.Atendimento.Queries;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Atendente")]
[Route("api/veiculos")]
public sealed class VeiculosController : ControllerBase
{
    private readonly IMediator _mediator;

    public VeiculosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarVeiculoCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { erro = result.Error });
        }

        return CreatedAtAction(nameof(Consultar), new { veiculoId = result.Value }, new { id = result.Value });
    }

    [HttpGet("{veiculoId:guid}")]
    public async Task<IActionResult> Consultar(Guid veiculoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConsultarVeiculoQuery(veiculoId), cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { erro = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> ListarPorCliente([FromQuery] Guid clienteId, CancellationToken cancellationToken)
    {
        var veiculos = await _mediator.Send(new ListarVeiculosPorClienteQuery(clienteId), cancellationToken);

        return Ok(veiculos);
    }

    [HttpPut("{veiculoId:guid}")]
    public async Task<IActionResult> Atualizar(Guid veiculoId, AtualizarVeiculoRequest request, CancellationToken cancellationToken)
    {
        var command = new AtualizarVeiculoCommand(veiculoId, request.Marca, request.Modelo, request.Ano);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { erro = result.Error });
        }

        return NoContent();
    }

    [HttpDelete("{veiculoId:guid}")]
    public async Task<IActionResult> Excluir(Guid veiculoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ExcluirVeiculoCommand(veiculoId), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { erro = result.Error });
        }

        return NoContent();
    }
}

public sealed record AtualizarVeiculoRequest(string Marca, string Modelo, int Ano);
