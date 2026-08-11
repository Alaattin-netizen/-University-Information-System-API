namespace UIS.Application.DTOs.Admin;

public class DepartmentResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? FacultyId { get; set; }
    public string FacultyName { get; set; }
    public int StudentCount { get; set; }
    public int InstructorCount { get; set; }
    public int CourseCount { get; set; }
    public DateTime CreatedAt { get; set; }
}