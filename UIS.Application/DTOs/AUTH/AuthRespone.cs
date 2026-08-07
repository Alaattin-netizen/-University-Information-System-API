namespace UIS.Application.DTOs.Auth;

public class AuthResponse
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}