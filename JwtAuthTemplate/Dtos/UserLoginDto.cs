using System.ComponentModel.DataAnnotations;

namespace JwtAuthTemplate.Dtos;

public sealed record UserLoginDto
{
    [Required(ErrorMessage = "Email address is required.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; init; } = string.Empty;
}
