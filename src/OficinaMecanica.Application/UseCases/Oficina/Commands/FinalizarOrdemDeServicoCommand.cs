using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed record FinalizarOrdemDeServicoCommand(Guid OrdemDeServicoId) : IRequest<Result>;
