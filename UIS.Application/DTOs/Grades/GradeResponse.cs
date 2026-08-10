namespace UIS.Application.DTOs.Grades;

public class GradeResponse
{
    public string CourseCode { get; set; }
    public string CourseName { get; set; }
    public int Credits { get; set; }
    public double? Midterm { get; set; }
    public double? Final { get; set; }
    public double? TotalScore { get; set; }
    public string LetterGrade { get; set; }
    public double GradePoint { get; set; }
}