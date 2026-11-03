using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Oficina.Queries;

public sealed record ConsultarStatusOrdemDeServicoQuery(Guid OrdemDeServicoId, Guid? ClienteId = null) : IRequest<Result<StatusOrdemDeServicoDto>>;
public sealed record StatusOrdemDeServicoDto(Guid Id, string Numero, string Status, DateTime DataUltimaAtualizacao, DateTime DataAbertura, DateTime? DataInicioExecucao, DateTime? DataFinalizacao);
