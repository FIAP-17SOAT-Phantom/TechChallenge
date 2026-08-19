using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.CatalogoServicos.Commands;

public sealed class AlterarStatusServicoHandler : IRequestHandler<AlterarStatusServicoCommand, Result>
{
    private readonly IServicoRepository _servicoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AlterarStatusServicoHandler(IServicoRepository servicoRepository, IUnitOfWork unitOfWork)
    {
        _servicoRepository = servicoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AlterarStatusServicoCommand request, CancellationToken cancellationToken)
    {
        var servico = await _servicoRepository.GetByIdAsync(request.ServicoId, cancellationToken);

        if (servico is null)
        {
            return Result.Failure("Servico nao encontrado");
        }

        if (request.Ativo)
        {
            servico.Ativar();
        }
        else
        {
            servico.Desativar();
        }

        _servicoRepository.Update(servico);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
