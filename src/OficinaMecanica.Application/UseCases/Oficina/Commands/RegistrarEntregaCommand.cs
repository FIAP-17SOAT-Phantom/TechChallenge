using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed record RegistrarEntregaCommand(Guid OrdemDeServicoId) : IRequest<Result>;
