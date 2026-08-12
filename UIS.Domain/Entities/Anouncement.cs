using System.ComponentModel.DataAnnotations;

namespace UIS.Domain.Entities;

public class Announcement 
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(200)]
    public string Title { get; set; }

    [Required]
    public string Content { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int CourseOfferingId { get; set; }
    public virtual CourseOffering CourseOffering { get; set; }

    public int InstructorId { get; set; }
    public virtual User Instructor { get; set; }
}