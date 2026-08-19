using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

public sealed record GerarOrcamentoCommand(Guid OrdemDeServicoId, string? Observacao) : IRequest<Result<Guid>>;
