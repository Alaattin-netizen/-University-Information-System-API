namespace UIS.Application.DTOs.Courses;

public class CourseResponse
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public int Credits { get; set; }
    public int Quota { get; set; }
    public int AvailableSlots { get; set; }
    public bool HasPrerequisite { get; set; }
    public string? PrerequisiteCode { get; set; }
}