using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.CatalogoServicos.Commands;

public sealed class AtualizarServicoHandler : IRequestHandler<AtualizarServicoCommand, Result>
{
    private readonly IServicoRepository _servicoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarServicoHandler(IServicoRepository servicoRepository, IUnitOfWork unitOfWork)
    {
        _servicoRepository = servicoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AtualizarServicoCommand request, CancellationToken cancellationToken)
    {
        var servico = await _servicoRepository.GetByIdAsync(request.ServicoId, cancellationToken);

        if (servico is null)
        {
            return Result.Failure("Servico nao encontrado");
        }

        servico.Atualizar(request.Nome, request.Descricao, request.PrecoBase, request.TempoEstimadoMinutos);

        _servicoRepository.Update(servico);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
