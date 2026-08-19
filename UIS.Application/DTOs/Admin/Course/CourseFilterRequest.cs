namespace UIS.Application.DTOs.Filters;

public class CourseFilterRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? DepartmentId { get; set; }
    public bool? IsMandatory { get; set; }
    public int? MinCredits { get; set; }
    public int? MaxCredits { get; set; }
    public int? MinECTS { get; set; }
    public int? MaxECTS { get; set; }
}