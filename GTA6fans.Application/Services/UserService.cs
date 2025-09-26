using GTA6fans.Application.DTOs;
using GTA6fans.Application.Interfaces;
using GTA6fans.Application.Mappers;
using GTA6fans.Domain.Interfaces;
using GTA6fans.Infrastructure.Utilities;

namespace GTA6fans.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public UserService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<UserDto> GetUserByIdAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }
        return UserMapper.ToDto(user);
    }

    public async Task<AuthenticateUserResponse> Login(AuthenticateUserRequest userRequest)
    {
        var user = await _userRepository.GetByEmailAsync(userRequest.Email);
        if (user == null || !PasswordHasher.VerifyPassword(userRequest.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var token = _jwtTokenGenerator.GenerateJwtToken(user.Id, (int)user.Type);

        return new AuthenticateUserResponse
        {
            Token = token,
            User = new UserDto
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Id = user.Id,
                UpdatedAt = user.UpdatedAt,
                CreatedAt = user.CreatedAt,
            }
        };
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        // Check if display name already exists
        if (!await _userRepository.IsDisplayNameAvailableAsync(request.DisplayName))
        {
            throw new InvalidOperationException("Display name is already taken.");
        }

        // Check if email already exists
        if (!await _userRepository.IsEmailAvailableAsync(request.Email))
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        // Hash the password (you should use a proper password hashing library like BCrypt)
        var passwordHash = PasswordHasher.HashPassword(request.Password);

        var user = UserMapper.ToEntity(request, passwordHash);
        var createdUser = await _userRepository.CreateAsync(user);

        return UserMapper.ToDto(createdUser);
    }

}