using MediatR;

namespace OficinaMecanica.Application.UseCases.Atendimento.Queries;

public sealed record ListarClientesQuery(int Pagina = 1, int TamanhoPagina = 20) : IRequest<IReadOnlyList<ClienteDto>>;
