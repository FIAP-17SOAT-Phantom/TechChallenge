using MediatR;
using OficinaMecanica.Application.Common.Interfaces;

namespace OficinaMecanica.Application.UseCases.Seguranca.Queries;

public sealed class ListarUsuariosHandler : IRequestHandler<ListarUsuariosQuery, IReadOnlyList<UsuarioDto>>
{
    private readonly IIdentityService _identityService;

    public ListarUsuariosHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<IReadOnlyList<UsuarioDto>> Handle(ListarUsuariosQuery request, CancellationToken cancellationToken) => await _identityService.ListarUsuariosAsync(request.Pagina, request.TamanhoPagina, cancellationToken);
}
