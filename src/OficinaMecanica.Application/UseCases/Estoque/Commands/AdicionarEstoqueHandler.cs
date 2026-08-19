using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Estoque.Commands;

public sealed class AdicionarEstoqueHandler : IRequestHandler<AdicionarEstoqueCommand, Result>
{
    private readonly IPecaRepository _pecaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdicionarEstoqueHandler(IPecaRepository pecaRepository, IUnitOfWork unitOfWork)
    {
        _pecaRepository = pecaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AdicionarEstoqueCommand request, CancellationToken cancellationToken)
    {
        var peca = await _pecaRepository.GetByIdAsync(request.PecaId, cancellationToken);

        if (peca is null)
        {
            return Result.Failure("Peca nao encontrada");
        }

        var result = peca.AdicionarEstoque(request.Quantidade);

        if (result.IsFailure)
        {
            return result;
        }

        _pecaRepository.Update(peca);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
