namespace JwtAuthTemplate.Configuration;

public sealed class JwtConfiguration
{
    public string? SecurityKey { get; init; }
    public string? ValidIssuer { get; init; }
    public string? ValidAudience { get; init; }
    public int ExpiresInMinutes { get; init; }
    public string? CookieName { get; init; }
}
