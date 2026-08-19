using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

public sealed record RejeitarOrcamentoCommand(Guid OrcamentoId) : IRequest<Result>;
