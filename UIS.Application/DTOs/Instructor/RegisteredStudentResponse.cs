namespace UIS.Application.DTOs.Instructor;

public class RegisteredStudentResponse
{
    public int StudentId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public double? MidtermScore { get; set; }
    public double? FinalScore { get; set; }
    public double? TotalScore { get; set; }
    public string LetterGrade { get; set; }
    public double? GradePoint { get; set; }
    public int AttendanceCount { get; set; }
    public int TotalClasses { get; set; }
}