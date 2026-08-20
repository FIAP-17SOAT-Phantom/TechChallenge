using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Orcamentacao.Enums;
using OficinaMecanica.Domain.Orcamentacao.ValueObjects;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

public sealed class AdicionarPecaOrcamentoHandler : IRequestHandler<AdicionarPecaOrcamentoCommand, Result>
{
    private readonly IOrcamentoRepository _orcamentoRepository;
    private readonly IPecaRepository _pecaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdicionarPecaOrcamentoHandler(IOrcamentoRepository orcamentoRepository, IPecaRepository pecaRepository, IUnitOfWork unitOfWork)
    {
        _orcamentoRepository = orcamentoRepository;
        _pecaRepository = pecaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AdicionarPecaOrcamentoCommand request, CancellationToken cancellationToken)
    {
        var orcamento = await _orcamentoRepository.GetByIdAsync(request.OrcamentoId, cancellationToken);

        if (orcamento is null)
        {
            return Result.NotFound("Orcamento nao encontrado");
        }

        var peca = await _pecaRepository.GetByIdAsync(request.PecaId, cancellationToken);

        if (peca is null)
        {
            return Result.NotFound("Peca nao encontrada");
        }

        var item = new ItemOrcamento(peca.Nome, TipoItem.Peca, request.Quantidade, peca.PrecoUnitario, pecaId: peca.Id);
        var result = orcamento.AdicionarItem(item);

        if (result.IsFailure)
        {
            return result;
        }

        _orcamentoRepository.Update(orcamento);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
