using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Integracoes.Commands;

public sealed record ProcessarRespostaExternaOrcamentoCommand(Guid OrcamentoId, string ExternalEventId, string Resposta, long TimestampUnix, string? Signature) : IRequest<Result<RespostaExternaOrcamentoDto>>;
public sealed record RespostaExternaOrcamentoDto(string ExternalEventId, bool Repetido);
