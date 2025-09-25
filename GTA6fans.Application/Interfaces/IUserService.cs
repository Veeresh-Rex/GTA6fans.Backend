using GTA6fans.Application.DTOs;

namespace GTA6fans.Application.Interfaces;

public interface IUserService
{
    Task<AuthenticateUserResponse> Login(AuthenticateUserRequest userRequest);

    Task<UserDto> CreateUserAsync(CreateUserRequest request);
}