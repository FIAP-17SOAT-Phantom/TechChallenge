using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Estoque.Commands;

public sealed class AtualizarPecaHandler : IRequestHandler<AtualizarPecaCommand, Result>
{
    private readonly IPecaRepository _pecaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarPecaHandler(IPecaRepository pecaRepository, IUnitOfWork unitOfWork)
    {
        _pecaRepository = pecaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AtualizarPecaCommand request, CancellationToken cancellationToken)
    {
        var peca = await _pecaRepository.GetByIdAsync(request.PecaId, cancellationToken);

        if (peca is null)
        {
            return Result.Failure("Peca nao encontrada");
        }

        peca.Atualizar(request.Nome, request.Descricao, request.PrecoUnitario, request.QuantidadeMinima);

        _pecaRepository.Update(peca);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
