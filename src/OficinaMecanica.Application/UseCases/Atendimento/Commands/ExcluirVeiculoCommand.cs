using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Atendimento.Commands;

public sealed record ExcluirVeiculoCommand(Guid VeiculoId) : IRequest<Result>;
