using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Seguranca.Commands;

public sealed class AutenticarHandler : IRequestHandler<AutenticarCommand, Result<TokenAcessoDto>>
{
    private readonly IIdentityService _identityService;

    public AutenticarHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<TokenAcessoDto>> Handle(AutenticarCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.AutenticarAsync(request.Email, request.Senha, cancellationToken);
    }
}
