using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Orcamentacao.Enums;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

public sealed class AprovarOrcamentoHandler : IRequestHandler<AprovarOrcamentoCommand, Result>
{
    private readonly IProcessadorRespostaOrcamento _processador;
    private readonly IUnitOfWork _unitOfWork;

    public AprovarOrcamentoHandler(IProcessadorRespostaOrcamento processador, IUnitOfWork unitOfWork)
    {
        _processador = processador;
        _unitOfWork = unitOfWork;
    }

    public Task<Result> Handle(AprovarOrcamentoCommand request, CancellationToken cancellationToken) => _unitOfWork.ExecuteInTransactionAsync(token => ProcessarAsync(request.OrcamentoId, token), cancellationToken);

    private async Task<Result> ProcessarAsync(Guid orcamentoId, CancellationToken cancellationToken)
    {
        var result = await _processador.ProcessAsync(orcamentoId, RespostaOrcamentoExterna.Aprovado, cancellationToken);

        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
