using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Seguranca.Queries;

public sealed record ConsultarUsuarioQuery(string UsuarioId) : IRequest<Result<UsuarioDto>>;
