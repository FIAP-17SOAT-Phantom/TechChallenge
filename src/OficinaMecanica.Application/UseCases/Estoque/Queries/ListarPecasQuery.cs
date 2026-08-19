using MediatR;

namespace OficinaMecanica.Application.UseCases.Estoque.Queries;

public sealed record ListarPecasQuery(bool SomenteEstoqueBaixo = false) : IRequest<IReadOnlyList<PecaDto>>;
