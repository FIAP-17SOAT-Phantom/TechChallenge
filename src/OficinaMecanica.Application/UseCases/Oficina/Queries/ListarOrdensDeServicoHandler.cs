using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Oficina.Entities;

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
        var ordensDeServico = await ObterOrdensAsync(request, cancellationToken);

        return ordensDeServico.Select(ordemDeServico =>
        {
            var itens = ordemDeServico.Itens.Select(item => new ItemOrdemDeServicoDto(item.ServicoId, item.PecaId, item.Quantidade, item.Observacao, item.Executado, item.DataExecucao)).ToList();
            return new OrdemDeServicoDto(ordemDeServico.Id, ordemDeServico.Numero, ordemDeServico.Status.ToString(), ordemDeServico.ClienteId, ordemDeServico.VeiculoId, ordemDeServico.MecanicoId, ordemDeServico.DataAbertura, ordemDeServico.DataInicioExecucao, ordemDeServico.DataFinalizacao, ordemDeServico.Diagnostico, ordemDeServico.OrcamentoId, itens);
        }).ToList();
    }

    private async Task<IReadOnlyList<OrdemDeServico>> ObterOrdensAsync(ListarOrdensDeServicoQuery request, CancellationToken cancellationToken)
    {
        if (request.ClienteId.HasValue)
        {
            return await _ordemDeServicoRepository.GetPagedByClienteIdAsync(request.ClienteId.Value, request.Status, request.Pagina, request.TamanhoPagina, cancellationToken);
        }

        if (request.Status.HasValue)
        {
            return await _ordemDeServicoRepository.GetByStatusAsync(request.Status.Value, cancellationToken);
        }

        return await _ordemDeServicoRepository.GetAllAsync(request.Pagina, request.TamanhoPagina, cancellationToken);
    }
}
