using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Atendimento.Commands;

public sealed class ExcluirVeiculoHandler : IRequestHandler<ExcluirVeiculoCommand, Result>
{
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExcluirVeiculoHandler(IVeiculoRepository veiculoRepository, IOrdemDeServicoRepository ordemDeServicoRepository, IUnitOfWork unitOfWork)
    {
        _veiculoRepository = veiculoRepository;
        _ordemDeServicoRepository = ordemDeServicoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ExcluirVeiculoCommand request, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoRepository.GetByIdAsync(request.VeiculoId, cancellationToken);

        if (veiculo is null)
        {
            return Result.NotFound("Veiculo nao encontrado");
        }

        if (await _ordemDeServicoRepository.ExistsByVeiculoIdAsync(request.VeiculoId, cancellationToken))
        {
            return Result.Conflict("Veiculo possui ordens de servico vinculadas");
        }

        _veiculoRepository.Remove(veiculo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
