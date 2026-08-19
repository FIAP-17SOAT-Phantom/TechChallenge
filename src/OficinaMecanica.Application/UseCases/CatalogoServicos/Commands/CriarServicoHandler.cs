using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.CatalogoServicos.Entities;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.CatalogoServicos.Commands;

public sealed class CriarServicoHandler : IRequestHandler<CriarServicoCommand, Result<Guid>>
{
    private readonly IServicoRepository _servicoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CriarServicoHandler(IServicoRepository servicoRepository, IUnitOfWork unitOfWork)
    {
        _servicoRepository = servicoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CriarServicoCommand request, CancellationToken cancellationToken)
    {
        var servico = new Servico(request.Nome, request.Descricao, request.PrecoBase, request.TempoEstimadoMinutos);

        await _servicoRepository.AddAsync(servico, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(servico.Id);
    }
}
