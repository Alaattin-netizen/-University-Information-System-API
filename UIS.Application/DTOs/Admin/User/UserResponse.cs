namespace UIS.Application.DTOs.Admin.User;

public class UserResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public int? DepartmentId { get; set; }
    public int? AdvisorId { get; set; } // For students only
   
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}