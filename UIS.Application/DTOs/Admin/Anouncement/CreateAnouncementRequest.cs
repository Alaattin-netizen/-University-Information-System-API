namespace UIS.Application.DTOs.Admin;

public class CreateAnnouncementRequest
{
    public string Title { get; set; }
    public string Content { get; set; }
    public int CourseOfferingId { get; set; }
}