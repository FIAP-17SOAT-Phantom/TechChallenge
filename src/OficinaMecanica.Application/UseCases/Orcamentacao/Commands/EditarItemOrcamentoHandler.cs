using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

public sealed class EditarItemOrcamentoHandler : IRequestHandler<EditarItemOrcamentoCommand, Result>
{
    private readonly IOrcamentoRepository _orcamentoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EditarItemOrcamentoHandler(IOrcamentoRepository orcamentoRepository, IUnitOfWork unitOfWork)
    {
        _orcamentoRepository = orcamentoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(EditarItemOrcamentoCommand request, CancellationToken cancellationToken)
    {
        var orcamento = await _orcamentoRepository.GetByIdAsync(request.OrcamentoId, cancellationToken);

        if (orcamento is null)
        {
            return Result.NotFound("Orcamento nao encontrado");
        }

        var result = request.Remover ? orcamento.RemoverItem(request.Tipo, request.ReferenciaId) : orcamento.AlterarQuantidadeItem(request.Tipo, request.ReferenciaId, request.Quantidade!.Value);

        if (result.IsFailure)
        {
            return result;
        }

        _orcamentoRepository.Update(orcamento);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
