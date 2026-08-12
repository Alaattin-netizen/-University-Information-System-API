using System.ComponentModel.DataAnnotations;

namespace UIS.Domain.Entities;

public class CourseOffering 
{
    [Key] public int Id { get; set; }  
    public int CourseId { get; set; }
    public virtual Course Course { get; set; }

    public int InstructorId { get; set; }
    public virtual User Instructor { get; set; } // ✅ Instructor is a User with role "Instructor"

    public int SemesterId { get; set; }
    public virtual Semester Semester { get; set; }

    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    [MaxLength(50)]
    public string Classroom { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
}