using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Seguranca.Commands;

public sealed record AutenticarCommand(string Email, string Senha) : IRequest<Result<TokenAcessoDto>>;
