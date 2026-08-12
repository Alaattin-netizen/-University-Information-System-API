namespace UIS.Application.DTOs.Admin.Course;

public class CourseResponse
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public int Credits { get; set; }
    public int ECTS { get; set; }
    public int Quota { get; set; }
    public bool IsMandatory { get; set; }
    public int? DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public int? PrerequisiteCourseId { get; set; }
    public string? PrerequisiteCode { get; set; }
    public int OfferingCount { get; set; }
    public DateTime CreatedAt { get; set; }
}