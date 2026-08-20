using MediatR;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Queries;

public sealed record ListarOrcamentosClienteQuery(Guid ClienteId, int Pagina = 1, int TamanhoPagina = 20) : IRequest<IReadOnlyList<OrcamentoDto>>;
