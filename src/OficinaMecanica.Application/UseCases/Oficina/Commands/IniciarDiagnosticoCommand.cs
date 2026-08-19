using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed record IniciarDiagnosticoCommand(Guid OrdemDeServicoId, Guid MecanicoId) : IRequest<Result>;
