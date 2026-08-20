using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed record RegistrarServicoExecutadoCommand(Guid OrdemDeServicoId, Guid ServicoId) : IRequest<Result>;
