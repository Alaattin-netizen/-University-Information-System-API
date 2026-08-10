namespace UIS.Application.Abstractions.StudentAbstractions;

public interface IJwtService
{
    string GenerateToken(int userId, string email, string role);
}