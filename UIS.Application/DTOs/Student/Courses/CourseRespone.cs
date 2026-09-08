namespace UIS.Application.DTOs.Student.Courses;

public class CourseResponse
{
    public int Id { get; set; }
    public int CourseOfferingId => Id;
    public string Code { get; set; }
    public string Name { get; set; }
    public int Credits { get; set; }
    public int Quota { get; set; }
    public int AvailableSlots { get; set; }
    public bool HasPrerequisite { get; set; }
    public string? PrerequisiteCode { get; set; }
    public string Day { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string Classroom { get; set; }
    public string InstructorName { get; set; }
}