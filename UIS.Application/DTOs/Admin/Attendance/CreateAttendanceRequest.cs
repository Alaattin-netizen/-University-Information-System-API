namespace UIS.Application.DTOs.Admin;

public class CreateAttendanceRequest
{
    public int StudentId { get; set; }
    public int CourseOfferingId { get; set; }
    public DateTime Date { get; set; }
    public bool IsPresent { get; set; }
}