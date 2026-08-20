using MediatR;
using OficinaMecanica.Application.Common.Interfaces;

namespace OficinaMecanica.Application.UseCases.Seguranca.Queries;

public sealed record ListarUsuariosQuery(int Pagina = 1, int TamanhoPagina = 20) : IRequest<IReadOnlyList<UsuarioDto>>;
