using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Oficina.Queries;

public sealed class ConsultarOrdemDeServicoHandler : IRequestHandler<ConsultarOrdemDeServicoQuery, Result<OrdemDeServicoDto>>
{
    private readonly IOrdemDeServicoRepository _osRepository;

    public ConsultarOrdemDeServicoHandler(IOrdemDeServicoRepository osRepository)
    {
        _osRepository = osRepository;
    }

    public async Task<Result<OrdemDeServicoDto>> Handle(ConsultarOrdemDeServicoQuery request, CancellationToken cancellationToken)
    {
        var os = await _osRepository.GetByIdAsync(request.OrdemDeServicoId, cancellationToken);

        if (os is null)
        {
            return Result.Failure<OrdemDeServicoDto>("Ordem de Servico nao encontrada");
        }

        var itens = os.Itens.Select(item => new ItemOrdemDeServicoDto(item.ServicoId, item.PecaId, item.Quantidade, item.Observacao)).ToList();
        var dto = new OrdemDeServicoDto(os.Id, os.Numero, os.Status.ToString(), os.ClienteId, os.VeiculoId, os.MecanicoId, os.DataAbertura, os.DataFinalizacao, os.Diagnostico, os.OrcamentoId, itens);

        return Result.Success(dto);
    }
}
