using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed record CancelarOrdemDeServicoCommand(Guid OrdemDeServicoId) : IRequest<Result>;
