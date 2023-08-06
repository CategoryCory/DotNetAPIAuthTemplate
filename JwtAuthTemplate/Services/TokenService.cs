#nullable disable

using JwtAuthTemplate.Configuration;
using JwtAuthTemplate.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuthTemplate.Services;

public sealed class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtConfiguration _jwtConfig;

    public TokenService(UserManager<ApplicationUser> userManager, IOptions<JwtConfiguration> jwtConfig)
    {
        _userManager = userManager;
        _jwtConfig = jwtConfig.Value;
    }

    public async Task<string> CreateTokenAsync(ApplicationUser user)
    {
        var claims = await GetClaimsAsync(user);

        var credentials = GetSigningCredentials();

        var tokenOptions = new JwtSecurityToken(
            issuer: _jwtConfig.ValidIssuer,
            audience: _jwtConfig.ValidAudience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtConfig.ExpiresInMinutes),
            signingCredentials: credentials
        );

        var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

        return token;
    }

    private async Task<List<Claim>> GetClaimsAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email),
        };

        var roles = await _userManager.GetRolesAsync(user);

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }

    private SigningCredentials GetSigningCredentials()
    {
        var tokenKey = Encoding.UTF8.GetBytes(_jwtConfig.SecurityKey);
        var securityKey = new SymmetricSecurityKey(tokenKey);

        return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);
    }
}
