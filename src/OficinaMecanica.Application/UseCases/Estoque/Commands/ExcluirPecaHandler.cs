using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Estoque.Commands;

public sealed class ExcluirPecaHandler : IRequestHandler<ExcluirPecaCommand, Result>
{
    private readonly IPecaRepository _pecaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExcluirPecaHandler(IPecaRepository pecaRepository, IUnitOfWork unitOfWork)
    {
        _pecaRepository = pecaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ExcluirPecaCommand request, CancellationToken cancellationToken)
    {
        var peca = await _pecaRepository.GetByIdAsync(request.PecaId, cancellationToken);

        if (peca is null)
        {
            return Result.Failure("Peca nao encontrada");
        }

        if (peca.Reservas.Count > 0)
        {
            return Result.Failure("Peca possui historico de reservas e nao pode ser excluida");
        }

        _pecaRepository.Remove(peca);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
