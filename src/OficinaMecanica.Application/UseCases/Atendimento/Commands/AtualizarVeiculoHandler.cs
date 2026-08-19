using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Atendimento.Commands;

public sealed class AtualizarVeiculoHandler : IRequestHandler<AtualizarVeiculoCommand, Result>
{
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarVeiculoHandler(IVeiculoRepository veiculoRepository, IUnitOfWork unitOfWork)
    {
        _veiculoRepository = veiculoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AtualizarVeiculoCommand request, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoRepository.GetByIdAsync(request.VeiculoId, cancellationToken);

        if (veiculo is null)
        {
            return Result.Failure("Veiculo nao encontrado");
        }

        veiculo.Atualizar(request.Marca, request.Modelo, request.Ano);

        _veiculoRepository.Update(veiculo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
