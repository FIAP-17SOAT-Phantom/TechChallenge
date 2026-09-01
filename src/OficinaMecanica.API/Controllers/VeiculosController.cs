using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.UseCases.Atendimento.Commands;
using OficinaMecanica.Application.UseCases.Atendimento.Queries;
using System.Text.Json.Serialization;
using OficinaMecanica.API.Extensions;

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
            return this.ToProblem(result);
        }

        return CreatedAtAction(nameof(Consultar), new { veiculoId = result.Value }, new { id = result.Value });
    }

    [HttpGet("{veiculoId:guid}")]
    public async Task<IActionResult> Consultar(Guid veiculoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConsultarVeiculoQuery(veiculoId), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> ListarPorCliente([FromQuery] Guid clienteId, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken cancellationToken = default)
    {
        var veiculos = await _mediator.Send(new ListarVeiculosPorClienteQuery(clienteId, pagina, tamanhoPagina), cancellationToken);

        return Ok(veiculos);
    }

    [HttpPut("{veiculoId:guid}")]
    public async Task<IActionResult> Atualizar(Guid veiculoId, AtualizarVeiculoRequest request, CancellationToken cancellationToken)
    {
        var command = new AtualizarVeiculoCommand(veiculoId, request.Marca, request.Modelo, request.Ano);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }

    [HttpDelete("{veiculoId:guid}")]
    public async Task<IActionResult> Excluir(Guid veiculoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ExcluirVeiculoCommand(veiculoId), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }
}

public sealed record AtualizarVeiculoRequest(string Marca, string Modelo, [property: JsonRequired] int Ano);
