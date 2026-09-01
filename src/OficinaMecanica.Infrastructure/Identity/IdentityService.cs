using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OficinaMecanica.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private const string UsuarioNaoEncontrado = "Usuario nao encontrado";
    private readonly UserManager<UsuarioSistema> _userManager;
    private readonly JwtOptions _jwtOptions;

    public IdentityService(UserManager<UsuarioSistema> userManager, IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Result<TokenAcessoDto>> AutenticarAsync(string email, string senha, CancellationToken cancellationToken = default)
    {
        var usuario = await _userManager.FindByEmailAsync(email);

        if (usuario is null || await _userManager.IsLockedOutAsync(usuario) || !await _userManager.CheckPasswordAsync(usuario, senha))
        {
            return Result.Unauthorized<TokenAcessoDto>("Email ou senha invalidos");
        }

        var roles = await _userManager.GetRolesAsync(usuario);
        var expiraEm = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id),
            new(JwtRegisteredClaimNames.Email, usuario.Email!),
            new(ClaimTypes.NameIdentifier, usuario.Id),
            new(ClaimTypes.Email, usuario.Email!)
        };

        if (usuario.ClienteId.HasValue)
        {
            claims.Add(new Claim("cliente_id", usuario.ClienteId.Value.ToString()));
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.Add(new Claim("troca_senha_obrigatoria", usuario.DeveAlterarSenha.ToString().ToLowerInvariant()));

        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_jwtOptions.Issuer, _jwtOptions.Audience, claims, expires: expiraEm, signingCredentials: credentials);
        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return Result.Success(new TokenAcessoDto(tokenValue, expiraEm, usuario.Email!, roles.ToList(), usuario.DeveAlterarSenha));
    }

    public async Task<Result<UsuarioCriadoDto>> CriarUsuarioAsync(string email, string role, Guid? clienteId, CancellationToken cancellationToken = default)
    {
        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return Result.Conflict<UsuarioCriadoDto>("Ja existe um usuario com este email");
        }

        if (clienteId.HasValue && await _userManager.Users.AnyAsync(usuario => usuario.ClienteId == clienteId.Value, cancellationToken))
        {
            return Result.Conflict<UsuarioCriadoDto>("Ja existe um usuario vinculado a este cliente");
        }

        var senhaTemporaria = GerarSenhaTemporaria();
        var usuario = new UsuarioSistema { UserName = email, Email = email, EmailConfirmed = true, ClienteId = clienteId, DeveAlterarSenha = true };
        var creationResult = await _userManager.CreateAsync(usuario, senhaTemporaria);

        if (!creationResult.Succeeded)
        {
            return Result.Failure<UsuarioCriadoDto>(string.Join(", ", creationResult.Errors.Select(error => error.Description)));
        }

        var roleResult = await _userManager.AddToRoleAsync(usuario, role);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(usuario);
            return Result.Failure<UsuarioCriadoDto>(string.Join(", ", roleResult.Errors.Select(error => error.Description)));
        }

        return Result.Success(new UsuarioCriadoDto(usuario.Id, senhaTemporaria));
    }

    public async Task<Result> AlterarSenhaAsync(string usuarioId, string senhaAtual, string novaSenha, CancellationToken cancellationToken = default)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId);

        if (usuario is null)
        {
            return Result.NotFound(UsuarioNaoEncontrado);
        }

        var changeResult = await _userManager.ChangePasswordAsync(usuario, senhaAtual, novaSenha);

        if (!changeResult.Succeeded)
        {
            return Result.Failure(string.Join(", ", changeResult.Errors.Select(error => error.Description)));
        }

        usuario.DeveAlterarSenha = false;
        var updateResult = await _userManager.UpdateAsync(usuario);

        if (!updateResult.Succeeded)
        {
            return Result.Failure(string.Join(", ", updateResult.Errors.Select(error => error.Description)));
        }

        return Result.Success();
    }

    public async Task<IReadOnlyList<UsuarioDto>> ListarUsuariosAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var usuarios = await _userManager.Users.OrderBy(usuario => usuario.Email).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var resultado = new List<UsuarioDto>();

        foreach (var usuario in usuarios)
        {
            resultado.Add(await MapearUsuarioAsync(usuario));
        }

        return resultado;
    }

    public async Task<Result<UsuarioDto>> ConsultarUsuarioAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId);

        if (usuario is null)
        {
            return Result.NotFound<UsuarioDto>(UsuarioNaoEncontrado);
        }

        return Result.Success(await MapearUsuarioAsync(usuario));
    }

    public async Task<Result> AlterarStatusUsuarioAsync(string usuarioId, bool ativo, CancellationToken cancellationToken = default)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId);

        if (usuario is null)
        {
            return Result.NotFound(UsuarioNaoEncontrado);
        }

        if (!ativo && !usuario.LockoutEnabled)
        {
            var lockoutResult = await _userManager.SetLockoutEnabledAsync(usuario, true);

            if (!lockoutResult.Succeeded)
            {
                return Result.Failure(string.Join(", ", lockoutResult.Errors.Select(error => error.Description)));
            }
        }

        var result = await _userManager.SetLockoutEndDateAsync(usuario, ativo ? null : DateTimeOffset.MaxValue);

        return result.Succeeded ? Result.Success() : Result.Failure(string.Join(", ", result.Errors.Select(error => error.Description)));
    }

    public async Task<Result<SenhaTemporariaDto>> RedefinirSenhaAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId);

        if (usuario is null)
        {
            return Result.NotFound<SenhaTemporariaDto>(UsuarioNaoEncontrado);
        }

        var senhaTemporaria = GerarSenhaTemporaria();
        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var resetResult = await _userManager.ResetPasswordAsync(usuario, token, senhaTemporaria);

        if (!resetResult.Succeeded)
        {
            return Result.Failure<SenhaTemporariaDto>(string.Join(", ", resetResult.Errors.Select(error => error.Description)));
        }

        usuario.DeveAlterarSenha = true;
        var updateResult = await _userManager.UpdateAsync(usuario);

        if (!updateResult.Succeeded)
        {
            return Result.Failure<SenhaTemporariaDto>(string.Join(", ", updateResult.Errors.Select(error => error.Description)));
        }

        return Result.Success(new SenhaTemporariaDto(senhaTemporaria));
    }

    public async Task<UsuarioAcessoDto?> ObterEstadoAcessoAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId);

        return usuario is null ? null : new UsuarioAcessoDto(!await _userManager.IsLockedOutAsync(usuario), usuario.DeveAlterarSenha);
    }

    public async Task<bool> ExisteUsuarioClienteAsync(Guid clienteId, CancellationToken cancellationToken = default) => await _userManager.Users.AnyAsync(usuario => usuario.ClienteId == clienteId, cancellationToken);

    private async Task<UsuarioDto> MapearUsuarioAsync(UsuarioSistema usuario)
    {
        var roles = await _userManager.GetRolesAsync(usuario);
        var ativo = !await _userManager.IsLockedOutAsync(usuario);

        return new UsuarioDto(usuario.Id, usuario.Email!, roles.ToList(), usuario.ClienteId, ativo, usuario.DeveAlterarSenha);
    }

    private static string GerarSenhaTemporaria()
    {
        const string caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%&*";
        var senha = new List<char> { 'A', 'a', '2', '!' };

        while (senha.Count < 16)
        {
            senha.Add(caracteres[RandomNumberGenerator.GetInt32(caracteres.Length)]);
        }

        for (var indice = senha.Count - 1; indice > 0; indice--)
        {
            var destino = RandomNumberGenerator.GetInt32(indice + 1);
            (senha[indice], senha[destino]) = (senha[destino], senha[indice]);
        }

        return new string(senha.ToArray());
    }
}
