using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

public sealed record AprovarOrcamentoCommand(Guid OrcamentoId) : IRequest<Result>;
