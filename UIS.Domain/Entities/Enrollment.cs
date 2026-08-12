using System.ComponentModel.DataAnnotations;

namespace UIS.Domain.Entities;

public class Enrollment 
{
    [Key] public int Id { get; set; }
    public int StudentId { get; set; }
    public virtual User Student { get; set; } // ✅ Student is a User with role "Student"

    public int CourseOfferingId { get; set; }
    public virtual CourseOffering CourseOffering { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Grade fields (merged)
    public double? MidtermScore { get; set; }
    public double? FinalScore { get; set; }
    public double? AssignmentScore { get; set; }
    public double? MakeupScore { get; set; }
    public double? TotalScore { get; set; }

    [MaxLength(2)]
    public string? LetterGrade { get; set; }

    public double? GradePoint { get; set; }
}