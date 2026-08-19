using MediatR;

namespace OficinaMecanica.Application.UseCases.Atendimento.Queries;

public sealed record ListarVeiculosPorClienteQuery(Guid ClienteId) : IRequest<IReadOnlyList<VeiculoDto>>;
