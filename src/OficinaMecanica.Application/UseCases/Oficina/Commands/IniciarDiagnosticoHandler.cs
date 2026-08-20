using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed class IniciarDiagnosticoHandler : IRequestHandler<IniciarDiagnosticoCommand, Result>
{
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public IniciarDiagnosticoHandler(IOrdemDeServicoRepository ordemDeServicoRepository, IUnitOfWork unitOfWork)
    {
        _ordemDeServicoRepository = ordemDeServicoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(IniciarDiagnosticoCommand request, CancellationToken cancellationToken)
    {
        var ordemDeServico = await _ordemDeServicoRepository.GetByIdAsync(request.OrdemDeServicoId, cancellationToken);

        if (ordemDeServico is null)
        {
            return Result.NotFound("Ordem de Servico nao encontrada");
        }

        var result = ordemDeServico.IniciarDiagnostico(request.MecanicoId);

        if (result.IsFailure)
        {
            return result;
        }

        _ordemDeServicoRepository.Update(ordemDeServico);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
