using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.UseCases.Orcamentacao.Commands;
using OficinaMecanica.Application.UseCases.Orcamentacao.Queries;
using OficinaMecanica.Application.UseCases.Oficina.Queries;
using System.Security.Claims;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/orcamentos")]
public sealed class OrcamentosController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrcamentosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> Gerar(GerarOrcamentoCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { erro = result.Error });
        }

        return CreatedAtAction(nameof(Consultar), new { orcamentoId = result.Value }, new { id = result.Value });
    }

    [HttpGet("{orcamentoId:guid}")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> Consultar(Guid orcamentoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConsultarOrcamentoQuery(orcamentoId), cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { erro = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("ordem-de-servico/{ordemDeServicoId:guid}")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> ConsultarPorOrdemDeServico(Guid ordemDeServicoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConsultarOrcamentoPorOrdemDeServicoQuery(ordemDeServicoId), cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { erro = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPatch("{orcamentoId:guid}/enviar")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> Enviar(Guid orcamentoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new EnviarOrcamentoCommand(orcamentoId), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { erro = result.Error });
        }

        return NoContent();
    }

    [HttpPatch("{orcamentoId:guid}/aprovar")]
    [Authorize(Roles = "Cliente")]
    public async Task<IActionResult> Aprovar(Guid orcamentoId, CancellationToken cancellationToken)
    {
        var authorizationResult = await ValidarPropriedadeDoOrcamentoAsync(orcamentoId, cancellationToken);

        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        var result = await _mediator.Send(new AprovarOrcamentoCommand(orcamentoId), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { erro = result.Error });
        }

        return NoContent();
    }

    [HttpPatch("{orcamentoId:guid}/rejeitar")]
    [Authorize(Roles = "Cliente")]
    public async Task<IActionResult> Rejeitar(Guid orcamentoId, CancellationToken cancellationToken)
    {
        var authorizationResult = await ValidarPropriedadeDoOrcamentoAsync(orcamentoId, cancellationToken);

        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        var result = await _mediator.Send(new RejeitarOrcamentoCommand(orcamentoId), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { erro = result.Error });
        }

        return NoContent();
    }

    [HttpGet("meus/{orcamentoId:guid}")]
    [Authorize(Roles = "Cliente")]
    public async Task<IActionResult> ConsultarMeuOrcamento(Guid orcamentoId, CancellationToken cancellationToken)
    {
        var authorizationResult = await ValidarPropriedadeDoOrcamentoAsync(orcamentoId, cancellationToken);

        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        var result = await _mediator.Send(new ConsultarOrcamentoQuery(orcamentoId), cancellationToken);

        return Ok(result.Value);
    }

    private async Task<IActionResult?> ValidarPropriedadeDoOrcamentoAsync(Guid orcamentoId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue("cliente_id"), out var clienteId))
        {
            return Forbid();
        }

        var orcamentoResult = await _mediator.Send(new ConsultarOrcamentoQuery(orcamentoId), cancellationToken);

        if (orcamentoResult.IsFailure)
        {
            return NotFound(new { erro = orcamentoResult.Error });
        }

        var ordemDeServicoResult = await _mediator.Send(new ConsultarOrdemDeServicoQuery(orcamentoResult.Value.OrdemDeServicoId), cancellationToken);

        if (ordemDeServicoResult.IsFailure)
        {
            return NotFound(new { erro = ordemDeServicoResult.Error });
        }

        if (ordemDeServicoResult.Value.ClienteId != clienteId)
        {
            return Forbid();
        }

        return null;
    }
}
