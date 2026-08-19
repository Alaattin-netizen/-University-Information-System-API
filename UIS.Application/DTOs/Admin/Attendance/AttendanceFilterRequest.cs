namespace UIS.Application.DTOs.Filters;

public class AttendanceFilterRequest
{
    public int? StudentId { get; set; }
    public int? CourseOfferingId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? IsPresent { get; set; }
}