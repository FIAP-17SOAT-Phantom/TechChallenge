using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed class RegistrarEntregaHandler : IRequestHandler<RegistrarEntregaCommand, Result>
{
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarEntregaHandler(IOrdemDeServicoRepository ordemDeServicoRepository, IUnitOfWork unitOfWork)
    {
        _ordemDeServicoRepository = ordemDeServicoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RegistrarEntregaCommand request, CancellationToken cancellationToken)
    {
        var ordemDeServico = await _ordemDeServicoRepository.GetByIdAsync(request.OrdemDeServicoId, cancellationToken);

        if (ordemDeServico is null)
        {
            return Result.Failure("Ordem de Servico nao encontrada");
        }

        var result = ordemDeServico.RegistrarEntrega();

        if (result.IsFailure)
        {
            return result;
        }

        _ordemDeServicoRepository.Update(ordemDeServico);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
