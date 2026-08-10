namespace UIS.Application.DTOs.Instructor;

public class AttendanceEntryRequest
{
    public int StudentId { get; set; }
    public DateTime Date { get; set; }
    public bool IsPresent { get; set; }
}