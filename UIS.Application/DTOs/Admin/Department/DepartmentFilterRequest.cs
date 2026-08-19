namespace UIS.Application.DTOs.Filters;

public class DepartmentFilterRequest
{
    public string? Name { get; set; }
    public int? FacultyId { get; set; }
    public string? FacultyName { get; set; } // Search by faculty name
}