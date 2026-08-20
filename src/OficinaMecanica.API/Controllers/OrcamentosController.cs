using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.UseCases.Orcamentacao.Commands;
using OficinaMecanica.Application.UseCases.Orcamentacao.Queries;
using OficinaMecanica.Application.UseCases.Oficina.Queries;
using OficinaMecanica.API.Extensions;
using OficinaMecanica.Domain.Orcamentacao.Enums;
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
            return this.ToProblem(result);
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
            return this.ToProblem(result);
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
            return this.ToProblem(result);
        }

        return Ok(result.Value);
    }

    [HttpPost("{orcamentoId:guid}/itens/servicos")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> AdicionarServico(Guid orcamentoId, AdicionarServicoOrcamentoRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AdicionarServicoOrcamentoCommand(orcamentoId, request.ServicoId, request.Quantidade), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }

    [HttpPost("{orcamentoId:guid}/itens/pecas")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> AdicionarPeca(Guid orcamentoId, AdicionarPecaOrcamentoRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AdicionarPecaOrcamentoCommand(orcamentoId, request.PecaId, request.Quantidade), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }

    [HttpPut("{orcamentoId:guid}/itens/servicos/{servicoId:guid}")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> AlterarQuantidadeServico(Guid orcamentoId, Guid servicoId, AlterarQuantidadeItemOrcamentoRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new EditarItemOrcamentoCommand(orcamentoId, TipoItem.Servico, servicoId, request.Quantidade, false), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }

    [HttpDelete("{orcamentoId:guid}/itens/servicos/{servicoId:guid}")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> RemoverServico(Guid orcamentoId, Guid servicoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new EditarItemOrcamentoCommand(orcamentoId, TipoItem.Servico, servicoId, null, true), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }

    [HttpPut("{orcamentoId:guid}/itens/pecas/{pecaId:guid}")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> AlterarQuantidadePeca(Guid orcamentoId, Guid pecaId, AlterarQuantidadeItemOrcamentoRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new EditarItemOrcamentoCommand(orcamentoId, TipoItem.Peca, pecaId, request.Quantidade, false), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }

    [HttpDelete("{orcamentoId:guid}/itens/pecas/{pecaId:guid}")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> RemoverPeca(Guid orcamentoId, Guid pecaId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new EditarItemOrcamentoCommand(orcamentoId, TipoItem.Peca, pecaId, null, true), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }

    [HttpPatch("{orcamentoId:guid}/enviar")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> Enviar(Guid orcamentoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new EnviarOrcamentoCommand(orcamentoId), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
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
            return this.ToProblem(result);
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
            return this.ToProblem(result);
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

    [HttpGet("meus")]
    [Authorize(Roles = "Cliente")]
    public async Task<IActionResult> ListarMeusOrcamentos([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(User.FindFirstValue("cliente_id"), out var clienteId))
        {
            return Forbid();
        }

        var orcamentos = await _mediator.Send(new ListarOrcamentosClienteQuery(clienteId, pagina, tamanhoPagina), cancellationToken);

        return Ok(orcamentos);
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
            return this.ToProblem(orcamentoResult);
        }

        var ordemDeServicoResult = await _mediator.Send(new ConsultarOrdemDeServicoQuery(orcamentoResult.Value.OrdemDeServicoId), cancellationToken);

        if (ordemDeServicoResult.IsFailure)
        {
            return this.ToProblem(ordemDeServicoResult);
        }

        if (ordemDeServicoResult.Value.ClienteId != clienteId)
        {
            return Forbid();
        }

        return null;
    }
}

public sealed record AdicionarServicoOrcamentoRequest(Guid ServicoId, int Quantidade);
public sealed record AdicionarPecaOrcamentoRequest(Guid PecaId, int Quantidade);
public sealed record AlterarQuantidadeItemOrcamentoRequest(int Quantidade);
