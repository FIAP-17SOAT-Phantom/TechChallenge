using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Seguranca.Commands;

public sealed record RedefinirSenhaUsuarioCommand(string UsuarioId) : IRequest<Result<SenhaTemporariaDto>>;
