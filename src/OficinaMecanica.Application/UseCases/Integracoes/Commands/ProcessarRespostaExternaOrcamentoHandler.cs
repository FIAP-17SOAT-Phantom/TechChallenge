using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Orcamentacao.Entities;
using OficinaMecanica.Domain.Orcamentacao.Enums;

namespace OficinaMecanica.Application.UseCases.Integracoes.Commands;

public sealed class ProcessarRespostaExternaOrcamentoHandler : IRequestHandler<ProcessarRespostaExternaOrcamentoCommand, Result<RespostaExternaOrcamentoDto>>
{
    private readonly IWebhookOrcamentoAuthenticator _authenticator;
    private readonly IEventoRespostaOrcamentoRepository _eventoRepository;
    private readonly IProcessadorRespostaOrcamento _processador;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessarRespostaExternaOrcamentoHandler(IWebhookOrcamentoAuthenticator authenticator, IEventoRespostaOrcamentoRepository eventoRepository, IProcessadorRespostaOrcamento processador, IUnitOfWork unitOfWork)
    {
        _authenticator = authenticator;
        _eventoRepository = eventoRepository;
        _processador = processador;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RespostaExternaOrcamentoDto>> Handle(ProcessarRespostaExternaOrcamentoCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<RespostaOrcamentoExterna>(request.Resposta, true, out var resposta) || !Enum.IsDefined(resposta))
            return Result.Failure<RespostaExternaOrcamentoDto>("Resposta deve ser Aprovado ou Rejeitado", ErrorType.Validation);

        var authenticationResult = _authenticator.Validate(request.ExternalEventId, request.TimestampUnix, request.OrcamentoId, resposta, request.Signature);

        if (authenticationResult.IsFailure)
            return Result.Failure<RespostaExternaOrcamentoDto>(authenticationResult.Error, authenticationResult.ErrorType);

        return await _unitOfWork.ExecuteInReadCommittedTransactionAsync(token => ProcessAsync(request, resposta, token), cancellationToken);
    }

    private async Task<Result<RespostaExternaOrcamentoDto>> ProcessAsync(ProcessarRespostaExternaOrcamentoCommand request, RespostaOrcamentoExterna resposta, CancellationToken cancellationToken)
    {
        await _eventoRepository.AcquireOrcamentoLockAsync(request.OrcamentoId, cancellationToken);
        var existingEvent = await _eventoRepository.GetByExternalEventIdAsync(request.ExternalEventId, cancellationToken);

        if (existingEvent is not null)
            return BuildRepeatedResult(existingEvent, request, resposta);

        var processResult = await _processador.ProcessAsync(request.OrcamentoId, resposta, cancellationToken);
        var evento = new EventoRespostaOrcamento(request.ExternalEventId, request.OrcamentoId, resposta, request.TimestampUnix, processResult);
        var inserted = await _eventoRepository.TryAddAsync(evento, cancellationToken);

        if (!inserted)
        {
            existingEvent = await _eventoRepository.GetByExternalEventIdAsync(request.ExternalEventId, cancellationToken);
            return existingEvent is null ? Result.Conflict<RespostaExternaOrcamentoDto>("Evento externo concorrente nao pode ser confirmado") : BuildRepeatedResult(existingEvent, request, resposta);
        }

        if (processResult.IsFailure)
            return Result.Failure<RespostaExternaOrcamentoDto>(processResult.Error, processResult.ErrorType);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new RespostaExternaOrcamentoDto(request.ExternalEventId, false));
    }

    private static Result<RespostaExternaOrcamentoDto> BuildRepeatedResult(EventoRespostaOrcamento existingEvent, ProcessarRespostaExternaOrcamentoCommand request, RespostaOrcamentoExterna resposta)
    {
        if (!existingEvent.Corresponde(request.OrcamentoId, resposta, request.TimestampUnix))
            return Result.Conflict<RespostaExternaOrcamentoDto>("ExternalEventId ja utilizado com outro conteudo");

        return existingEvent.ProcessadoComSucesso ? Result.Success(new RespostaExternaOrcamentoDto(request.ExternalEventId, true)) : Result.Failure<RespostaExternaOrcamentoDto>(existingEvent.Erro ?? "Evento externo processado anteriormente com falha", existingEvent.TipoErro ?? ErrorType.BusinessRule);
    }
}
