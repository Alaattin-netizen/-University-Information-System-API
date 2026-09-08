namespace UIS.Application.DTOs.Student.Grades;

public class GradeResponse
{
    public string CourseCode { get; set; }
    public string CourseName { get; set; }
    public int Credits { get; set; }
    public double? Midterm { get; set; }
    public double? Assignment { get; set; }
    public double? Makeup { get; set; }
    public double? Final { get; set; }
    public double? TotalScore { get; set; }
    public string? LetterGrade { get; set; }
    public double GradePoint { get; set; }
    public string SemesterName { get; set; }
}