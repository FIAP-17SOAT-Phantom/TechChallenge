using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Queries;

public sealed record ConsultarOrcamentoQuery(Guid OrcamentoId) : IRequest<Result<OrcamentoDto>>;
