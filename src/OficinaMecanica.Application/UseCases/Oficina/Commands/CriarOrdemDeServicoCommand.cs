using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed record CriarOrdemDeServicoCommand(Guid ClienteId, Guid VeiculoId) : IRequest<Result<Guid>>;
