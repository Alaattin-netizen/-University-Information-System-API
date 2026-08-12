namespace UIS.Application.DTOs.Admin;

public class CourseOfferingResponse
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseCode { get; set; }
    public string CourseName { get; set; }
    public int InstructorId { get; set; }
    public string InstructorName { get; set; }
    public int SemesterId { get; set; }
    public string SemesterName { get; set; }
    public string Day { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string Classroom { get; set; }
    public int EnrolledCount { get; set; }
}