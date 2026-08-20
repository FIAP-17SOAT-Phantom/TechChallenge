using MediatR;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Orcamentacao.Enums;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

public sealed record EditarItemOrcamentoCommand(Guid OrcamentoId, TipoItem Tipo, Guid ReferenciaId, int? Quantidade, bool Remover) : IRequest<Result>;
