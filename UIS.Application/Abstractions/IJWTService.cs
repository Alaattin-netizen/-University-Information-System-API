namespace UIS.Application.Abstractions;

public interface IJwtService
{
    string GenerateToken(int userId, string email, List<string> roles);
}