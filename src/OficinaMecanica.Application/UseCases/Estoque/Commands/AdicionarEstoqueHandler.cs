using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Estoque.Commands;

public sealed class AdicionarEstoqueHandler : IRequestHandler<AdicionarEstoqueCommand, Result>
{
    private readonly IPecaRepository _pecaRepository;
    private readonly IAlertaEstoqueRepository _alertaEstoqueRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdicionarEstoqueHandler(IPecaRepository pecaRepository, IAlertaEstoqueRepository alertaEstoqueRepository, IUnitOfWork unitOfWork)
    {
        _pecaRepository = pecaRepository;
        _alertaEstoqueRepository = alertaEstoqueRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AdicionarEstoqueCommand request, CancellationToken cancellationToken)
    {
        var peca = await _pecaRepository.GetByIdAsync(request.PecaId, cancellationToken);

        if (peca is null)
        {
            return Result.NotFound("Peca nao encontrada");
        }

        var result = peca.AdicionarEstoque(request.Quantidade);

        if (result.IsFailure)
        {
            return result;
        }

        if (peca.QuantidadeDisponivel > peca.QuantidadeMinima)
        {
            var alerta = await _alertaEstoqueRepository.GetAtivoByPecaIdAsync(peca.Id, cancellationToken);

            if (alerta is not null)
            {
                var resolucaoResult = alerta.Resolver();

                if (resolucaoResult.IsFailure)
                {
                    return resolucaoResult;
                }

                _alertaEstoqueRepository.Update(alerta);
            }
        }

        _pecaRepository.Update(peca);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
