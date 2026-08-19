using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

public sealed class EnviarOrcamentoHandler : IRequestHandler<EnviarOrcamentoCommand, Result>
{
    private readonly IOrcamentoRepository _orcamentoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EnviarOrcamentoHandler(IOrcamentoRepository orcamentoRepository, IUnitOfWork unitOfWork)
    {
        _orcamentoRepository = orcamentoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(EnviarOrcamentoCommand request, CancellationToken cancellationToken)
    {
        var orcamento = await _orcamentoRepository.GetByIdAsync(request.OrcamentoId, cancellationToken);

        if (orcamento is null)
        {
            return Result.Failure("Orcamento nao encontrado");
        }

        var result = orcamento.Enviar();

        if (result.IsFailure)
        {
            return result;
        }

        _orcamentoRepository.Update(orcamento);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
