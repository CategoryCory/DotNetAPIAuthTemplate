namespace JwtAuthTemplate.Dtos;

public sealed record UserProfileDto
{
    public string? Id { get; set; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
}
