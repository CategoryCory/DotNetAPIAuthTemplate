using JwtAuthTemplate.Data.Models;

namespace JwtAuthTemplate.Services;
public interface ITokenService
{
    Task<string> CreateTokenAsync(ApplicationUser user);
}