using MediatR;
using OficinaMecanica.Application.Common.Interfaces;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Queries;

public sealed class ListarOrcamentosClienteHandler : IRequestHandler<ListarOrcamentosClienteQuery, IReadOnlyList<OrcamentoDto>>
{
    private readonly IOrcamentoRepository _orcamentoRepository;

    public ListarOrcamentosClienteHandler(IOrcamentoRepository orcamentoRepository)
    {
        _orcamentoRepository = orcamentoRepository;
    }

    public async Task<IReadOnlyList<OrcamentoDto>> Handle(ListarOrcamentosClienteQuery request, CancellationToken cancellationToken)
    {
        var orcamentos = await _orcamentoRepository.GetPagedByClienteIdAsync(request.ClienteId, request.Pagina, request.TamanhoPagina, cancellationToken);

        return orcamentos.Select(orcamento => new OrcamentoDto(orcamento.Id, orcamento.OrdemDeServicoId, orcamento.Versao, orcamento.Status.ToString(), orcamento.ValorTotal, orcamento.DataCriacao, orcamento.DataAprovacao, orcamento.Observacao, orcamento.Itens.Select(item => new ItemOrcamentoDto(item.Descricao, item.Tipo.ToString(), item.Quantidade, item.ValorUnitario, item.ValorTotal, item.PecaId, item.ServicoId)).ToList())).ToList();
    }
}
