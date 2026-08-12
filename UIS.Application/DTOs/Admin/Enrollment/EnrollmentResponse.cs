namespace UIS.Application.DTOs.Admin;

public class EnrollmentResponse
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; }
    public int CourseOfferingId { get; set; }
    public string CourseCode { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public bool IsActive { get; set; }
    public double? MidtermScore { get; set; }
    public double? FinalScore { get; set; }
    public double? TotalScore { get; set; }
    public string LetterGrade { get; set; }
    public double? GradePoint { get; set; }
}