using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.UseCases.Atendimento.Commands;
using OficinaMecanica.Application.UseCases.Atendimento.Queries;
using OficinaMecanica.API.Extensions;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Atendente")]
[Route("api/clientes")]
public sealed class ClientesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarClienteCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return CreatedAtAction(nameof(Consultar), new { clienteId = result.Value }, new { id = result.Value });
    }

    [HttpGet("{clienteId:guid}")]
    public async Task<IActionResult> Consultar(Guid clienteId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConsultarClienteQuery(clienteId), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken cancellationToken = default)
    {
        var clientes = await _mediator.Send(new ListarClientesQuery(pagina, tamanhoPagina), cancellationToken);

        return Ok(clientes);
    }

    [HttpPut("{clienteId:guid}")]
    public async Task<IActionResult> Atualizar(Guid clienteId, AtualizarClienteRequest request, CancellationToken cancellationToken)
    {
        var command = new AtualizarClienteCommand(clienteId, request.Nome, request.Telefone, request.Email);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }

    [HttpDelete("{clienteId:guid}")]
    public async Task<IActionResult> Excluir(Guid clienteId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ExcluirClienteCommand(clienteId), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }
}

public sealed record AtualizarClienteRequest(string Nome, string Telefone, string Email);
