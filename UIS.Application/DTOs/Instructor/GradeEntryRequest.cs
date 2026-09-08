using System.ComponentModel.DataAnnotations;

namespace UIS.Application.DTOs.Instructor;

public class GradeEntryRequest
{
    public int EnrollmentId { get; set; }
    [Range(0, 100)]
    public double? MidtermScore { get; set; }
    [Range(0, 100)]
    public double? FinalScore { get; set; }
    [Range(0, 100)]
    public double? AssignmentScore { get; set; }
    [Range(0, 100)]
    public double? MakeupScore { get; set; }
}