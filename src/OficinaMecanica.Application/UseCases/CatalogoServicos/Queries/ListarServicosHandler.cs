using MediatR;
using OficinaMecanica.Application.Common.Interfaces;

namespace OficinaMecanica.Application.UseCases.CatalogoServicos.Queries;

public sealed class ListarServicosHandler : IRequestHandler<ListarServicosQuery, IReadOnlyList<ServicoDto>>
{
    private readonly IServicoRepository _servicoRepository;

    public ListarServicosHandler(IServicoRepository servicoRepository)
    {
        _servicoRepository = servicoRepository;
    }

    public async Task<IReadOnlyList<ServicoDto>> Handle(ListarServicosQuery request, CancellationToken cancellationToken)
    {
        var servicos = await _servicoRepository.GetPagedAsync(request.SomenteAtivos, request.Pagina, request.TamanhoPagina, cancellationToken);

        return servicos
            .Select(servico => new ServicoDto(
                servico.Id,
                servico.Nome,
                servico.Descricao,
                servico.PrecoBase,
                servico.TempoEstimadoMinutos,
                servico.Ativo))
            .ToList();
    }
}
