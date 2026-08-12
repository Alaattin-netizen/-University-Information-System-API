namespace UIS.Application.DTOs.Admin;

public class AttendanceResponse
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; }
    public int CourseOfferingId { get; set; }
    public string CourseCode { get; set; }
    public DateTime Date { get; set; }
    public bool IsPresent { get; set; }
}