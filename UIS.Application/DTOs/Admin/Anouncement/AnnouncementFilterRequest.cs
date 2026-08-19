namespace UIS.Application.DTOs.Filters;

public class AnnouncementFilterRequest
{
    public int? CourseOfferingId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}