using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.UseCases.Estoque.Commands;
using OficinaMecanica.Application.UseCases.Estoque.Queries;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/pecas")]
public sealed class PecasController : ControllerBase
{
    private readonly IMediator _mediator;

    public PecasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarPecaCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { erro = result.Error });
        }

        return CreatedAtAction(nameof(Consultar), new { pecaId = result.Value }, new { id = result.Value });
    }

    [HttpGet("{pecaId:guid}")]
    public async Task<IActionResult> Consultar(Guid pecaId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConsultarPecaQuery(pecaId), cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { erro = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool somenteEstoqueBaixo = false, CancellationToken cancellationToken = default)
    {
        var pecas = await _mediator.Send(new ListarPecasQuery(somenteEstoqueBaixo), cancellationToken);

        return Ok(pecas);
    }

    [HttpPut("{pecaId:guid}")]
    public async Task<IActionResult> Atualizar(Guid pecaId, AtualizarPecaRequest request, CancellationToken cancellationToken)
    {
        var command = new AtualizarPecaCommand(pecaId, request.Nome, request.Descricao, request.PrecoUnitario, request.QuantidadeMinima);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { erro = result.Error });
        }

        return NoContent();
    }

    [HttpPatch("{pecaId:guid}/estoque")]
    public async Task<IActionResult> AdicionarEstoque(Guid pecaId, AdicionarEstoqueRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AdicionarEstoqueCommand(pecaId, request.Quantidade), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { erro = result.Error });
        }

        return NoContent();
    }

    [HttpDelete("{pecaId:guid}")]
    public async Task<IActionResult> Excluir(Guid pecaId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ExcluirPecaCommand(pecaId), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { erro = result.Error });
        }

        return NoContent();
    }
}

public sealed record AtualizarPecaRequest(string Nome, string Descricao, decimal PrecoUnitario, int QuantidadeMinima);
public sealed record AdicionarEstoqueRequest(int Quantidade);
