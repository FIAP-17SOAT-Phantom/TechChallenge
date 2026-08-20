using MediatR;

namespace OficinaMecanica.Application.UseCases.CatalogoServicos.Queries;

public sealed record ListarServicosQuery(bool SomenteAtivos = true, int Pagina = 1, int TamanhoPagina = 20) : IRequest<IReadOnlyList<ServicoDto>>;
