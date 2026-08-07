using System.ComponentModel.DataAnnotations;

namespace UIS.Domain.Entities;

public class Announcement 
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(200)]
    public string Title { get; set; }

    [Required]
    public string Content { get; set; } // Can be long, so no MaxLength

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Optional: If you want to allow instructors to schedule future announcements
    public DateTime? PublishDate { get; set; }

    // Foreign Keys
    public int CourseOfferingId { get; set; }
    public virtual CourseOffering CourseOffering { get; set; }

    public int InstructorId { get; set; }
    public virtual Instructor Instructor { get; set; }
}