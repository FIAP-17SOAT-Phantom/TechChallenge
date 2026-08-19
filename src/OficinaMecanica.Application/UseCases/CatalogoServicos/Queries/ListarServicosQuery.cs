using MediatR;

namespace OficinaMecanica.Application.UseCases.CatalogoServicos.Queries;

public sealed record ListarServicosQuery(bool SomenteAtivos = true) : IRequest<IReadOnlyList<ServicoDto>>;
