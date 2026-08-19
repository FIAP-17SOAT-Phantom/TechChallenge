using MediatR;
using OficinaMecanica.Application.Common.Interfaces;

namespace OficinaMecanica.Application.UseCases.Oficina.Queries;

public sealed class ListarOrdensDeServicoHandler : IRequestHandler<ListarOrdensDeServicoQuery, IReadOnlyList<OrdemDeServicoDto>>
{
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;

    public ListarOrdensDeServicoHandler(IOrdemDeServicoRepository ordemDeServicoRepository)
    {
        _ordemDeServicoRepository = ordemDeServicoRepository;
    }

    public async Task<IReadOnlyList<OrdemDeServicoDto>> Handle(ListarOrdensDeServicoQuery request, CancellationToken cancellationToken)
    {
        var ordensDeServico = request.ClienteId.HasValue
            ? await _ordemDeServicoRepository.GetByClienteIdAsync(request.ClienteId.Value, cancellationToken)
            : request.Status.HasValue
                ? await _ordemDeServicoRepository.GetByStatusAsync(request.Status.Value, cancellationToken)
                : await _ordemDeServicoRepository.GetAllAsync(request.Pagina, request.TamanhoPagina, cancellationToken);

        return ordensDeServico.Select(ordemDeServico =>
        {
            var itens = ordemDeServico.Itens.Select(item => new ItemOrdemDeServicoDto(item.ServicoId, item.PecaId, item.Quantidade, item.Observacao)).ToList();
            return new OrdemDeServicoDto(ordemDeServico.Id, ordemDeServico.Numero, ordemDeServico.Status.ToString(), ordemDeServico.ClienteId, ordemDeServico.VeiculoId, ordemDeServico.MecanicoId, ordemDeServico.DataAbertura, ordemDeServico.DataFinalizacao, ordemDeServico.Diagnostico, ordemDeServico.OrcamentoId, itens);
        }).ToList();
    }
}
