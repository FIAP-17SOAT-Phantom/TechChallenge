using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Queries;

public sealed record ConsultarOrcamentoPorOrdemDeServicoQuery(Guid OrdemDeServicoId) : IRequest<Result<OrcamentoDto>>;
