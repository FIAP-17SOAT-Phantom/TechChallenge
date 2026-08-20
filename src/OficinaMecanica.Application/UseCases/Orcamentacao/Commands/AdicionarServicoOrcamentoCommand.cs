using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

public sealed record AdicionarServicoOrcamentoCommand(Guid OrcamentoId, Guid ServicoId, int Quantidade) : IRequest<Result>;
