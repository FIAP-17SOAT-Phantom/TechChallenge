using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<TokenAcessoDto>> AutenticarAsync(string email, string senha, CancellationToken cancellationToken = default);
    Task<Result<UsuarioCriadoDto>> CriarUsuarioAsync(string email, string role, Guid? clienteId, CancellationToken cancellationToken = default);
    Task<Result> AlterarSenhaAsync(string usuarioId, string senhaAtual, string novaSenha, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsuarioDto>> ListarUsuariosAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<UsuarioDto>> ConsultarUsuarioAsync(string usuarioId, CancellationToken cancellationToken = default);
    Task<Result> AlterarStatusUsuarioAsync(string usuarioId, bool ativo, CancellationToken cancellationToken = default);
    Task<Result<SenhaTemporariaDto>> RedefinirSenhaAsync(string usuarioId, CancellationToken cancellationToken = default);
    Task<UsuarioAcessoDto?> ObterEstadoAcessoAsync(string usuarioId, CancellationToken cancellationToken = default);
    Task<bool> ExisteUsuarioClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
}

public sealed record TokenAcessoDto(string Token, DateTime ExpiraEm, string Email, IReadOnlyList<string> Roles, bool TrocaSenhaObrigatoria);
public sealed record UsuarioCriadoDto(string UsuarioId, string SenhaTemporaria);
public sealed record UsuarioDto(string Id, string Email, IReadOnlyList<string> Roles, Guid? ClienteId, bool Ativo, bool TrocaSenhaObrigatoria);
public sealed record SenhaTemporariaDto(string SenhaTemporaria);
public sealed record UsuarioAcessoDto(bool Ativo, bool TrocaSenhaObrigatoria);
