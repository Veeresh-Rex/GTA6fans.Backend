namespace GTA6fans.Domain.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateJwtToken(string username, int role);
}
