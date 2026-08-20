using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Estoque.Commands;

public sealed class AlterarStatusAlertaEstoqueHandler : IRequestHandler<AlterarStatusAlertaEstoqueCommand, Result>
{
    private readonly IAlertaEstoqueRepository _alertaEstoqueRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AlterarStatusAlertaEstoqueHandler(IAlertaEstoqueRepository alertaEstoqueRepository, IUnitOfWork unitOfWork)
    {
        _alertaEstoqueRepository = alertaEstoqueRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AlterarStatusAlertaEstoqueCommand request, CancellationToken cancellationToken)
    {
        var alerta = await _alertaEstoqueRepository.GetByIdAsync(request.AlertaId, cancellationToken);

        if (alerta is null)
        {
            return Result.NotFound("Alerta de estoque nao encontrado");
        }

        var result = request.Resolver ? alerta.Resolver() : alerta.MarcarComoVisualizado();

        if (result.IsFailure)
        {
            return result;
        }

        _alertaEstoqueRepository.Update(alerta);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
