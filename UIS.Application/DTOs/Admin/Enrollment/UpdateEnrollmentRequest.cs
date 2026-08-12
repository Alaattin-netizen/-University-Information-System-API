namespace UIS.Application.DTOs.Admin;

public class UpdateEnrollmentRequest
{
    public int Id { get; set; }
    public double? MidtermScore { get; set; }
    public double? FinalScore { get; set; }
    public double? TotalScore { get; set; }
    public string LetterGrade { get; set; }
    public double? GradePoint { get; set; }
}