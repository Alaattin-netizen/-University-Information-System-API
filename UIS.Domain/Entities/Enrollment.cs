using System.ComponentModel.DataAnnotations;
using UIS.Domain.Entities.Users;

namespace UIS.Domain.Entities;

public class Enrollment 
{
    [Key] public int Id { get; set; }
    public int StudentId { get; set; }
    public virtual Student Student { get; set; }

    public int CourseOfferingId { get; set; }
    public virtual CourseOffering CourseOffering { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Raw scores (nullable because they might not be entered yet)
    public double? MidtermScore { get; set; }
    public double? FinalScore { get; set; }
    public double? AssignmentScore { get; set; }
    public double? MakeupScore { get; set; }

    // Calculated weighted total (will be set by Application layer)
    public double? TotalScore { get; set; }

    // The Letter Grade (e.g., "AA", "BB")
    [MaxLength(2)]
    public string? LetterGrade { get; set; }

    // Grade Point Value (e.g., 4.0 for AA, 3.0 for BA)
    public double? GradePoint { get; set; }
}