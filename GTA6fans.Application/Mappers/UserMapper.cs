using GTA6fans.Application.DTOs;
using GTA6fans.Domain.Entities;
using GTA6fans.Domain.Enums;

namespace GTA6fans.Application.Mappers;

public static class UserMapper
{
    public static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public static User ToEntity(CreateUserRequest request, string passwordHash)
    {
        return new User
        {
            DisplayName = request.DisplayName,
            Email = request.Email,
            PasswordHash = passwordHash,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static void UpdateEntity(User user, UpdateUserRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.DisplayName))
            user.DisplayName = request.DisplayName;

        if (!string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email;

        if (request.Status.HasValue)
            user.Status = request.Status.Value;
    }
}
