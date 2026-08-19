using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Atendimento.Commands;

public sealed record AtualizarClienteCommand(Guid ClienteId, string Nome, string Telefone, string Email) : IRequest<Result>;
