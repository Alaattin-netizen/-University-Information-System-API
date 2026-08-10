namespace UIS.Application.DTOs.Grades;

public class GPAResponse
{
    public double SemesterGPA { get; set; }
    public double CumulativeGPA { get; set; }
    public int TotalCredits { get; set; }
}