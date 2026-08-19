using MediatR;
using OficinaMecanica.Domain.Oficina.Enums;

namespace OficinaMecanica.Application.UseCases.Oficina.Queries;

public sealed record ListarOrdensDeServicoQuery(Guid? ClienteId, StatusOS? Status, int Pagina = 1, int TamanhoPagina = 20) : IRequest<IReadOnlyList<OrdemDeServicoDto>>;
