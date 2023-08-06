using JwtAuthTemplate.Data.Models;
using System.Data;

namespace JwtAuthTemplate.Dtos;

public sealed record UserLoginResponseDto
{
    public bool IsAuthenticationSuccessful { get; init; }
    public string? ErrorMessage { get; init; }
    public string? UserId { get; init; }
    public string? UserName { get; init; }
    public string? DisplayName { get; init; }
    public string? Email { get; init; }
    public IList<string>? Roles { get; init; }

    public static UserLoginResponseDto CreateFromUser(ApplicationUser user, IList<string> roles)
    {
        return new UserLoginResponseDto
        {
            IsAuthenticationSuccessful = true,
            ErrorMessage = string.Empty,
            UserId = user.Id,
            DisplayName = user.FirstName,
            UserName = user.UserName,
            Email = user.Email,
            Roles = roles,
        };
    }
}
