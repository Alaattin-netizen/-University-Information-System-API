namespace UIS.Application.DTOs.Auth;
using UIS.Domain.Enums;
public class AuthResponse
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public Role Role { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}