using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Seguranca.Commands;

public sealed record AlterarStatusUsuarioCommand(string UsuarioId, bool Ativo, string UsuarioSolicitanteId) : IRequest<Result>;
