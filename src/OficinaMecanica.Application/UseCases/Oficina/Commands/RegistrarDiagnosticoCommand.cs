using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed record RegistrarDiagnosticoCommand(Guid OrdemDeServicoId, string Diagnostico, IReadOnlyList<ItemDiagnosticoRequest> Itens) : IRequest<Result>;

public sealed record ItemDiagnosticoRequest(Guid ServicoId, Guid? PecaId, int Quantidade, string? Observacao);
