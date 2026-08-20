using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Orcamentacao.Enums;
using OficinaMecanica.Domain.Orcamentacao.ValueObjects;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

public sealed class AdicionarServicoOrcamentoHandler : IRequestHandler<AdicionarServicoOrcamentoCommand, Result>
{
    private readonly IOrcamentoRepository _orcamentoRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdicionarServicoOrcamentoHandler(IOrcamentoRepository orcamentoRepository, IServicoRepository servicoRepository, IUnitOfWork unitOfWork)
    {
        _orcamentoRepository = orcamentoRepository;
        _servicoRepository = servicoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AdicionarServicoOrcamentoCommand request, CancellationToken cancellationToken)
    {
        var orcamento = await _orcamentoRepository.GetByIdAsync(request.OrcamentoId, cancellationToken);

        if (orcamento is null)
        {
            return Result.NotFound("Orcamento nao encontrado");
        }

        var servico = await _servicoRepository.GetByIdAsync(request.ServicoId, cancellationToken);

        if (servico is null || !servico.Ativo)
        {
            return Result.NotFound("Servico nao encontrado ou inativo");
        }

        var item = new ItemOrcamento(servico.Nome, TipoItem.Servico, request.Quantidade, servico.PrecoBase, servicoId: servico.Id);
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
