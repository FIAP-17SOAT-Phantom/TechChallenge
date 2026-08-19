using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<TokenAcessoDto>> AutenticarAsync(string email, string senha, CancellationToken cancellationToken = default);
    Task<Result<string>> CriarUsuarioAsync(string email, string senha, string role, Guid? clienteId, CancellationToken cancellationToken = default);
}

public sealed record TokenAcessoDto(string Token, DateTime ExpiraEm, string Email, IReadOnlyList<string> Roles);
