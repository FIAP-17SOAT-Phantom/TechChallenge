using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Seguranca.Commands;

public sealed class AlterarStatusUsuarioHandler : IRequestHandler<AlterarStatusUsuarioCommand, Result>
{
    private readonly IIdentityService _identityService;

    public AlterarStatusUsuarioHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(AlterarStatusUsuarioCommand request, CancellationToken cancellationToken)
    {
        if (!request.Ativo && request.UsuarioId == request.UsuarioSolicitanteId)
        {
            return Result.Failure("O administrador nao pode desativar o proprio usuario");
        }

        return await _identityService.AlterarStatusUsuarioAsync(request.UsuarioId, request.Ativo, cancellationToken);
    }
}
