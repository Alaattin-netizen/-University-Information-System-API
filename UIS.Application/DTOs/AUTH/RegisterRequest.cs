using UIS.Domain.Enums;

namespace UIS.Application.DTOs.Auth;

public class RegisterRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public Role Role { get; set; }
    public int? DepartmentId { get; set; }
}