namespace UIS.Application.DTOs.Student.Schedule;

public class ScheduleResponse
{
    public string Day { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string CourseCode { get; set; }
    public string CourseName { get; set; }
    public string Instructor { get; set; }
    public string Classroom { get; set; }
}