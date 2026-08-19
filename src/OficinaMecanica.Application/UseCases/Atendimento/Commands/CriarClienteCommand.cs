using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Atendimento.Commands;

public sealed record CriarClienteCommand(string Nome, string Cpf, string Telefone, string Email) : IRequest<Result<Guid>>;
