using MediatR;

namespace OficinaMecanica.Application.UseCases.Estoque.Queries;

public sealed record ListarPecasQuery(bool SomenteEstoqueBaixo = false, int Pagina = 1, int TamanhoPagina = 20) : IRequest<IReadOnlyList<PecaDto>>;
