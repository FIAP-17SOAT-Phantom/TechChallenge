using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Seguranca.Commands;

public sealed class RedefinirSenhaUsuarioHandler : IRequestHandler<RedefinirSenhaUsuarioCommand, Result<SenhaTemporariaDto>>
{
    private readonly IIdentityService _identityService;

    public RedefinirSenhaUsuarioHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<SenhaTemporariaDto>> Handle(RedefinirSenhaUsuarioCommand request, CancellationToken cancellationToken) => await _identityService.RedefinirSenhaAsync(request.UsuarioId, cancellationToken);
}
