namespace UIS.Application.DTOs.Filters;

public class UserFilterRequest
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public List<string>? Roles { get; set; } // e.g., ["Student", "Instructor"]
    public int? DepartmentId { get; set; }
}