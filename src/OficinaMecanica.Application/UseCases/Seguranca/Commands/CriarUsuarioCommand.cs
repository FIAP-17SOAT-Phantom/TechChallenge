using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Seguranca.Commands;

public sealed record CriarUsuarioCommand(string Email, string Role, Guid? ClienteId) : IRequest<Result<UsuarioCriadoDto>>;
