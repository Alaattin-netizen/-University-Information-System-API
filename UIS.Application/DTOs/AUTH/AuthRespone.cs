using System.Text.Json.Serialization;

namespace UIS.Application.DTOs.Auth;
public class AuthResponse
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    [JsonIgnore]
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}