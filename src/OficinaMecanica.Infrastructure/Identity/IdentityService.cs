using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OficinaMecanica.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
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

        if (usuario is null || !await _userManager.CheckPasswordAsync(usuario, senha))
        {
            return Result.Failure<TokenAcessoDto>("Email ou senha invalidos");
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

        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_jwtOptions.Issuer, _jwtOptions.Audience, claims, expires: expiraEm, signingCredentials: credentials);
        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return Result.Success(new TokenAcessoDto(tokenValue, expiraEm, usuario.Email!, roles.ToList()));
    }

    public async Task<Result<string>> CriarUsuarioAsync(string email, string senha, string role, Guid? clienteId, CancellationToken cancellationToken = default)
    {
        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return Result.Failure<string>("Ja existe um usuario com este email");
        }

        var usuario = new UsuarioSistema { UserName = email, Email = email, EmailConfirmed = true, ClienteId = clienteId };
        var creationResult = await _userManager.CreateAsync(usuario, senha);

        if (!creationResult.Succeeded)
        {
            return Result.Failure<string>(string.Join(", ", creationResult.Errors.Select(error => error.Description)));
        }

        var roleResult = await _userManager.AddToRoleAsync(usuario, role);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(usuario);
            return Result.Failure<string>(string.Join(", ", roleResult.Errors.Select(error => error.Description)));
        }

        return Result.Success(usuario.Id);
    }
}
