using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Atendimento.Commands;

public sealed record ExcluirClienteCommand(Guid ClienteId) : IRequest<Result>;
