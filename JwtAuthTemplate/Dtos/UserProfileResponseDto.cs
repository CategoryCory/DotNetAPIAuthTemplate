using JwtAuthTemplate.Data.Models;

namespace JwtAuthTemplate.Dtos;

public sealed record UserProfileResponseDto
{
    public string? Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }

    public static UserProfileResponseDto CreateFromUser(ApplicationUser user)
    {
        return new UserProfileResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };
    }

    public static List<UserProfileResponseDto> CreateFromUserList(List<ApplicationUser> users)
    {
        var usersList = new List<UserProfileResponseDto>();

        foreach (var user in users)
        {
            usersList.Add(CreateFromUser(user));
        }

        return usersList;
    }
}
