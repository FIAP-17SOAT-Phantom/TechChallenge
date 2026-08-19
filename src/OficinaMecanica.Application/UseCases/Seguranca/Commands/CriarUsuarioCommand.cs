using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Seguranca.Commands;

public sealed record CriarUsuarioCommand(string Email, string Senha, string Role, Guid? ClienteId) : IRequest<Result<string>>;
