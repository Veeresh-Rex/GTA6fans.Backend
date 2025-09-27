using System.Net.Http.Json;
using GTA6fans.Application.DTOs;
using GTA6fans.Application.Interfaces;
using GTA6fans.Application.Mappers;
using GTA6fans.Domain.Interfaces;
using GTA6fans.Infrastructure.Utilities;
using Microsoft.Extensions.Configuration;

namespace GTA6fans.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public UserService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator, IConfiguration configuration, HttpClient httpClient)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _configuration = configuration;
        _httpClient = httpClient;
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

    public async Task<AuthenticateUserResponse> CreateUserAsync(CreateUserRequest request)
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
        var token = _jwtTokenGenerator.GenerateJwtToken(user.Id, (int)user.Type);

        return new AuthenticateUserResponse
        {
            Token = token,
            User = UserMapper.ToDto(createdUser)
        };
    }

    public async Task<bool> VerifyCaptcha(VerifyCaptchaRequest request)
    {

        var secret = _configuration["ReCaptcha:SecretKey"];
        var verifyUrl = $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={request.Token}";

        var response = await _httpClient.GetAsync(verifyUrl);


        var captchaResult = await response.Content.ReadFromJsonAsync<GoogleCaptchaResponse>();

        if (captchaResult == null)
        {
            throw new UnauthorizedAccessException("Captcha verification failed.");
        }

        return captchaResult.Success;
    }
}