using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Seguranca.Commands;

public sealed class CriarUsuarioHandler : IRequestHandler<CriarUsuarioCommand, Result<UsuarioCriadoDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IClienteRepository _clienteRepository;

    public CriarUsuarioHandler(IIdentityService identityService, IClienteRepository clienteRepository)
    {
        _identityService = identityService;
        _clienteRepository = clienteRepository;
    }

    public async Task<Result<UsuarioCriadoDto>> Handle(CriarUsuarioCommand request, CancellationToken cancellationToken)
    {
        if (request.Role == "Cliente")
        {
            var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId!.Value, cancellationToken);

            if (cliente is null)
            {
                return Result.NotFound<UsuarioCriadoDto>("Cliente nao encontrado");
            }
        }

        return await _identityService.CriarUsuarioAsync(request.Email, request.Role, request.ClienteId, cancellationToken);
    }
}
