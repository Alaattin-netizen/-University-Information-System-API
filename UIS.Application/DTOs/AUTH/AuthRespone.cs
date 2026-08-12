namespace UIS.Application.DTOs.Auth;
public class AuthResponse
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}