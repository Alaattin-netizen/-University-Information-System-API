namespace UIS.Application.DTOs.Admin.Attendance;

public class AttendanceImportResult
{
    public int Created { get; set; }
    public int Updated { get; set; }
    public List<string> Errors { get; set; } = new();
}
