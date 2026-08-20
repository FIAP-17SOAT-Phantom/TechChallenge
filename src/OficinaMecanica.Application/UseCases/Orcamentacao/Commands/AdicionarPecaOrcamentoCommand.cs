using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

public sealed record AdicionarPecaOrcamentoCommand(Guid OrcamentoId, Guid PecaId, int Quantidade) : IRequest<Result>;
