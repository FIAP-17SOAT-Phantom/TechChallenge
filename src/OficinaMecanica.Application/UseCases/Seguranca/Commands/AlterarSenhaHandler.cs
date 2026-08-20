using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Seguranca.Commands;

public sealed class AlterarSenhaHandler : IRequestHandler<AlterarSenhaCommand, Result>
{
    private readonly IIdentityService _identityService;

    public AlterarSenhaHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(AlterarSenhaCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.AlterarSenhaAsync(request.UsuarioId, request.SenhaAtual, request.NovaSenha, cancellationToken);
    }
}
