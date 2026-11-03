using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Oficina.Queries;

public sealed class ConsultarStatusOrdemDeServicoHandler : IRequestHandler<ConsultarStatusOrdemDeServicoQuery, Result<StatusOrdemDeServicoDto>>
{
    private readonly IOrdemDeServicoRepository _repository;

    public ConsultarStatusOrdemDeServicoHandler(IOrdemDeServicoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<StatusOrdemDeServicoDto>> Handle(ConsultarStatusOrdemDeServicoQuery request, CancellationToken cancellationToken)
    {
        var ordemDeServico = await _repository.GetByIdAsync(request.OrdemDeServicoId, cancellationToken);

        if (ordemDeServico is null)
        {
            return Result.NotFound<StatusOrdemDeServicoDto>("Ordem de Servico nao encontrada");
        }

        if (request.ClienteId.HasValue && ordemDeServico.ClienteId != request.ClienteId.Value)
        {
            return Result.Failure<StatusOrdemDeServicoDto>("A Ordem de Servico nao pertence ao cliente autenticado", ErrorType.Forbidden);
        }

        return Result.Success(new StatusOrdemDeServicoDto(ordemDeServico.Id, ordemDeServico.Numero, ordemDeServico.Status.ToString(), ordemDeServico.DataUltimaAtualizacao, ordemDeServico.DataAbertura, ordemDeServico.DataInicioExecucao, ordemDeServico.DataFinalizacao));
    }
}
