namespace UIS.Application.DTOs.Admin;

public class AnnouncementResponse
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedDate { get; set; }
    public int CourseOfferingId { get; set; }
    public int InstructorId { get; set; }
    public string InstructorName { get; set; }
    public string CourseCode { get; set; }
}