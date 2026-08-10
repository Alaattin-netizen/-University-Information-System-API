namespace UIS.Application.DTOs.Instructor;

public class CreateAnnouncementRequest
{
    public int CourseOfferingId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
}