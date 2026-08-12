namespace UIS.Application.DTOs.Admin;

public class UpdateCourseOfferingRequest
{
    public int Id { get; set; }
    public int? InstructorId { get; set; }
    public int? SemesterId { get; set; }
    public int? Day { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string Classroom { get; set; }
}