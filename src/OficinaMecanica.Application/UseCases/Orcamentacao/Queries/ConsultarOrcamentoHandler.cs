using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Queries;

public sealed class ConsultarOrcamentoHandler : IRequestHandler<ConsultarOrcamentoQuery, Result<OrcamentoDto>>
{
    private readonly IOrcamentoRepository _orcamentoRepository;

    public ConsultarOrcamentoHandler(IOrcamentoRepository orcamentoRepository)
    {
        _orcamentoRepository = orcamentoRepository;
    }

    public async Task<Result<OrcamentoDto>> Handle(ConsultarOrcamentoQuery request, CancellationToken cancellationToken)
    {
        var orcamento = await _orcamentoRepository.GetByIdAsync(request.OrcamentoId, cancellationToken);

        if (orcamento is null)
        {
            return Result.Failure<OrcamentoDto>("Orcamento nao encontrado");
        }

        var itens = orcamento.Itens.Select(item => new ItemOrcamentoDto(item.Descricao, item.Tipo.ToString(), item.Quantidade, item.ValorUnitario, item.ValorTotal, item.PecaId, item.ServicoId)).ToList();
        var dto = new OrcamentoDto(orcamento.Id, orcamento.OrdemDeServicoId, orcamento.Versao, orcamento.Status.ToString(), orcamento.ValorTotal, orcamento.DataCriacao, orcamento.DataAprovacao, orcamento.Observacao, itens);

        return Result.Success(dto);
    }
}
