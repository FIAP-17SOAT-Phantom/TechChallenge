using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.API.Extensions;
using OficinaMecanica.Application.UseCases.Integracoes.Commands;

namespace OficinaMecanica.API.Controllers;

[ApiController]
[Route("api/webhooks/orcamentos")]
public sealed class WebhooksController : ControllerBase
{
    private readonly IMediator _mediator;

    public WebhooksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{orcamentoId:guid}/resposta")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RespostaExternaOrcamentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ProcessarRespostaOrcamento(Guid orcamentoId, ProcessarRespostaOrcamentoRequest request, [FromHeader(Name = "X-Webhook-Signature")] string? signature, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ProcessarRespostaExternaOrcamentoCommand(orcamentoId, request.ExternalEventId, request.Resposta, request.TimestampUnix, signature), cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result);

        return Ok(result.Value);
    }
}

public sealed record ProcessarRespostaOrcamentoRequest(string ExternalEventId, string Resposta, long TimestampUnix);
