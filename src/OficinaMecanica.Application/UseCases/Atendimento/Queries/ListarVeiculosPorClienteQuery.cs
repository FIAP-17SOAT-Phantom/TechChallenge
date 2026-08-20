using MediatR;

namespace OficinaMecanica.Application.UseCases.Atendimento.Queries;

public sealed record ListarVeiculosPorClienteQuery(Guid ClienteId, int Pagina = 1, int TamanhoPagina = 20) : IRequest<IReadOnlyList<VeiculoDto>>;
