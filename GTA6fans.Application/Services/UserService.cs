using System.Net.Http.Json;
using GTA6fans.Application.DTOs;
using GTA6fans.Application.Interfaces;
using GTA6fans.Application.Mappers;
using GTA6fans.Domain.Interfaces;
using GTA6fans.Domain.Models;
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
        // Verify CAPTCHA
        if (!await VerifyCaptcha(request.RecaptchaToken))
        {
            throw new UnauthorizedAccessException("Captcha verification failed.");
        }

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

    private async Task<bool> VerifyCaptcha(string recaptchaToken)
    {

        var secret = _configuration["RecaptchaSettings:SecretKey"];
        var url = _configuration["RecaptchaSettings:VerificationUrl"];

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("secret", secret),
            new KeyValuePair<string, string>("response", recaptchaToken),
        });

        var response = await _httpClient.PostAsync(new Uri(url), content);
        response.EnsureSuccessStatusCode();

        var captchaResult = await response.Content.ReadFromJsonAsync<RecaptchaVerificationResponse>();

        if (captchaResult == null)
        {
            throw new UnauthorizedAccessException("Captcha verification failed.");
        }

        return captchaResult.Success;
    }
}