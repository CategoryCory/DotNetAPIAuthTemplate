using System.ComponentModel.DataAnnotations;

namespace JwtAuthTemplate.Dtos;

public sealed record UserRegistrationDto
{
    [Required(ErrorMessage = "Email address is required.")]
    [MaxLength(256)]
    public string? Email { get; init; }

    [Required(ErrorMessage = "Password is required.")]
    public string? Password { get; init; }

    [Compare("Password", ErrorMessage = "The password and confirmation password must match.")]
    public string? ConfirmPassword { get; init; }
}
