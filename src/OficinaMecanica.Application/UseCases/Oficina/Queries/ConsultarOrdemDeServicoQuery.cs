using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Oficina.Queries;

public sealed record ConsultarOrdemDeServicoQuery(Guid OrdemDeServicoId) : IRequest<Result<OrdemDeServicoDto>>;

public sealed record OrdemDeServicoDto(
 Guid Id,
 string Numero,
 string Status,
 Guid ClienteId,
 Guid VeiculoId,
 Guid? MecanicoId,
 DateTime DataAbertura,
 DateTime? DataFinalizacao,
 string? Diagnostico);
