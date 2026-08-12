namespace UIS.Application.DTOs.Admin;

public class CreateCourseOfferingRequest
{
    public int CourseId { get; set; }
    public int InstructorId { get; set; }
    public int SemesterId { get; set; }
    public int Day { get; set; } // 0=Sunday, 1=Monday...
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string Classroom { get; set; }
}