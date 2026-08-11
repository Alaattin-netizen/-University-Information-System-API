namespace UIS.Application.DTOs.Admin;

public class FacultyResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? DeanName { get; set; }
    public int DepartmentCount { get; set; }
    public DateTime CreatedAt { get; set; }
}