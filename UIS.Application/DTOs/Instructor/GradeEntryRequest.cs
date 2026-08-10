namespace UIS.Application.DTOs.Instructor;

public class GradeEntryRequest
{
    public int EnrollmentId { get; set; }
    public double? MidtermScore { get; set; }
    public double? FinalScore { get; set; }
    public double? AssignmentScore { get; set; }
    public double? MakeupScore { get; set; }
}