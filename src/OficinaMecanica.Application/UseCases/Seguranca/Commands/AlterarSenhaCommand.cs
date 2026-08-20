using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Seguranca.Commands;

public sealed record AlterarSenhaCommand(string UsuarioId, string SenhaAtual, string NovaSenha) : IRequest<Result>;
