using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.UseCases.Oficina.Commands;
using OficinaMecanica.Application.UseCases.Oficina.Queries;
using OficinaMecanica.API.Extensions;
using OficinaMecanica.Domain.Oficina.Enums;
using System.Text.Json.Serialization;
using System.Security.Claims;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/ordens-de-servico")]
public sealed class OrdensDeServicoController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdensDeServicoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> Criar(CriarOrdemDeServicoCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return CreatedAtAction(nameof(Consultar), new { ordemDeServicoId = result.Value }, new { id = result.Value });
    }

    [HttpPost("abertura")]
    [Authorize(Roles = "Admin,Atendente")]
    [ProducesResponseType(typeof(OrdemDeServicoCriadaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Abrir(AbrirOrdemDeServicoCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return CreatedAtAction(nameof(Consultar), new { ordemDeServicoId = result.Value.Id }, result.Value);
    }

    [HttpGet("{ordemDeServicoId:guid}")]
    [Authorize(Roles = "Admin,Atendente,Mecanico")]
    public async Task<IActionResult> Consultar(Guid ordemDeServicoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConsultarOrdemDeServicoQuery(ordemDeServicoId), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{ordemDeServicoId:guid}/status")]
    [Authorize(Roles = "Admin,Atendente,Mecanico")]
    [ProducesResponseType(typeof(StatusOrdemDeServicoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConsultarStatus(Guid ordemDeServicoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConsultarStatusOrdemDeServicoQuery(ordemDeServicoId), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return Ok(result.Value);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Atendente,Mecanico")]
    public async Task<IActionResult> Listar([FromQuery] Guid? clienteId, [FromQuery] StatusOS? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken cancellationToken = default)
    {
        var ordensDeServico = await _mediator.Send(new ListarOrdensDeServicoQuery(clienteId, status, pagina, tamanhoPagina), cancellationToken);

        return Ok(ordensDeServico);
    }

    [HttpGet("fila-operacional")]
    [Authorize(Roles = "Admin,Atendente,Mecanico")]
    [ProducesResponseType(typeof(IReadOnlyList<OrdemDeServicoOperacionalDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarFilaOperacional([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken cancellationToken = default)
    {
        var ordensDeServico = await _mediator.Send(new ListarFilaOperacionalQuery(pagina, tamanhoPagina), cancellationToken);
        return Ok(ordensDeServico);
    }

    [HttpGet("indicadores")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ObterIndicadores(CancellationToken cancellationToken)
    {
        var indicadores = await _mediator.Send(new ObterIndicadoresOficinaQuery(), cancellationToken);

        return Ok(indicadores);
    }

    [HttpPatch("{ordemDeServicoId:guid}/iniciar-diagnostico")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> IniciarDiagnostico(Guid ordemDeServicoId, IniciarDiagnosticoRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new IniciarDiagnosticoCommand(ordemDeServicoId, request.MecanicoId), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }

    [HttpPatch("{ordemDeServicoId:guid}/registrar-diagnostico")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> RegistrarDiagnostico(Guid ordemDeServicoId, RegistrarDiagnosticoRequest request, CancellationToken cancellationToken)
    {
        var command = new RegistrarDiagnosticoCommand(ordemDeServicoId, request.Diagnostico, request.Itens);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }

    [HttpPatch("{ordemDeServicoId:guid}/finalizar")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> Finalizar(Guid ordemDeServicoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new FinalizarOrdemDeServicoCommand(ordemDeServicoId), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }

    [HttpPatch("{ordemDeServicoId:guid}/servicos/{servicoId:guid}/executar")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> RegistrarServicoExecutado(Guid ordemDeServicoId, Guid servicoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RegistrarServicoExecutadoCommand(ordemDeServicoId, servicoId), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }

    [HttpPatch("{ordemDeServicoId:guid}/entregar")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> RegistrarEntrega(Guid ordemDeServicoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RegistrarEntregaCommand(ordemDeServicoId), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }

    [HttpPatch("{ordemDeServicoId:guid}/cancelar")]
    [Authorize(Roles = "Admin,Atendente")]
    public async Task<IActionResult> Cancelar(Guid ordemDeServicoId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelarOrdemDeServicoCommand(ordemDeServicoId), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return NoContent();
    }

    [HttpGet("minhas/{ordemDeServicoId:guid}")]
    [Authorize(Roles = "Cliente")]
    public async Task<IActionResult> ConsultarMinhaOrdemDeServico(Guid ordemDeServicoId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue("cliente_id"), out var clienteId))
        {
            return Forbid();
        }

        var result = await _mediator.Send(new ConsultarOrdemDeServicoQuery(ordemDeServicoId), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        if (result.Value.ClienteId != clienteId)
        {
            return Forbid();
        }

        return Ok(result.Value);
    }

    [HttpGet("minhas/{ordemDeServicoId:guid}/status")]
    [Authorize(Roles = "Cliente")]
    [ProducesResponseType(typeof(StatusOrdemDeServicoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConsultarStatusMinhaOrdemDeServico(Guid ordemDeServicoId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue("cliente_id"), out var clienteId))
        {
            return Forbid();
        }

        var result = await _mediator.Send(new ConsultarStatusOrdemDeServicoQuery(ordemDeServicoId, clienteId), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("minhas")]
    [Authorize(Roles = "Cliente")]
    public async Task<IActionResult> ListarMinhasOrdensDeServico([FromQuery] StatusOS? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(User.FindFirstValue("cliente_id"), out var clienteId))
        {
            return Forbid();
        }

        var ordensDeServico = await _mediator.Send(new ListarOrdensDeServicoQuery(clienteId, status, pagina, tamanhoPagina), cancellationToken);

        return Ok(ordensDeServico);
    }
}

public sealed record IniciarDiagnosticoRequest([property: JsonRequired] Guid MecanicoId);
public sealed record RegistrarDiagnosticoRequest(string Diagnostico, IReadOnlyList<ItemDiagnosticoRequest> Itens);
