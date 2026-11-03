using MediatR;
using OficinaMecanica.Application.Common.Interfaces;

namespace OficinaMecanica.Application.UseCases.Oficina.Queries;

public sealed class ListarFilaOperacionalHandler : IRequestHandler<ListarFilaOperacionalQuery, IReadOnlyList<OrdemDeServicoOperacionalDto>>
{
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;

    public ListarFilaOperacionalHandler(IOrdemDeServicoRepository ordemDeServicoRepository) => _ordemDeServicoRepository = ordemDeServicoRepository;

    public async Task<IReadOnlyList<OrdemDeServicoOperacionalDto>> Handle(ListarFilaOperacionalQuery request, CancellationToken cancellationToken)
    {
        var ordens = await _ordemDeServicoRepository.GetFilaOperacionalAsync(request.Pagina, request.TamanhoPagina, cancellationToken);
        return ordens.Select(ordem => new OrdemDeServicoOperacionalDto(ordem.Id, ordem.Numero, ordem.Status.ToString(), ordem.ClienteId, ordem.VeiculoId, ordem.MecanicoId, ordem.DataAbertura)).ToList();
    }
}
