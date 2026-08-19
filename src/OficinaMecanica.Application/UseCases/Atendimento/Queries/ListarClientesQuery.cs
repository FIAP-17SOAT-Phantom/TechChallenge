using MediatR;

namespace OficinaMecanica.Application.UseCases.Atendimento.Queries;

public sealed record ListarClientesQuery : IRequest<IReadOnlyList<ClienteDto>>;
