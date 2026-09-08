namespace UIS.Application.DTOs.Admin.User;

public class CreateUserRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public List<string> Roles { get; set; } = new();
    public int? DepartmentId { get; set; }
    public int? AdvisorId { get; set; }
}
