namespace UIS.Application.DTOs.Admin.User
    ;

public class UpdateUserRequest
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public int? DepartmentId { get; set; }
    public int? AdvisorId { get; set; } 
}