using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed class RegistrarServicoExecutadoHandler : IRequestHandler<RegistrarServicoExecutadoCommand, Result>
{
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarServicoExecutadoHandler(IOrdemDeServicoRepository ordemDeServicoRepository, IUnitOfWork unitOfWork)
    {
        _ordemDeServicoRepository = ordemDeServicoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RegistrarServicoExecutadoCommand request, CancellationToken cancellationToken)
    {
        var ordemDeServico = await _ordemDeServicoRepository.GetByIdAsync(request.OrdemDeServicoId, cancellationToken);

        if (ordemDeServico is null)
        {
            return Result.NotFound("Ordem de Servico nao encontrada");
        }

        var result = ordemDeServico.RegistrarServicoExecutado(request.ServicoId);

        if (result.IsFailure)
        {
            return result;
        }

        _ordemDeServicoRepository.Update(ordemDeServico);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
