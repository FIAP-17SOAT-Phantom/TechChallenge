using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Seguranca.Queries;

public sealed class ConsultarUsuarioHandler : IRequestHandler<ConsultarUsuarioQuery, Result<UsuarioDto>>
{
    private readonly IIdentityService _identityService;

    public ConsultarUsuarioHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<UsuarioDto>> Handle(ConsultarUsuarioQuery request, CancellationToken cancellationToken) => await _identityService.ConsultarUsuarioAsync(request.UsuarioId, cancellationToken);
}
