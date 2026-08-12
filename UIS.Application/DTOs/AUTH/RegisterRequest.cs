
namespace UIS.Application.DTOs.Auth;

public class RegisterRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public int? DepartmentId { get; set; }
}