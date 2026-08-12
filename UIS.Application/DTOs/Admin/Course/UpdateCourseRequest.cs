namespace UIS.Application.DTOs.Admin.Course;

public class UpdateCourseRequest
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public int Credits { get; set; }
    public int ECTS { get; set; }
    public int Quota { get; set; }
    public bool IsMandatory { get; set; }
    public int DepartmentId { get; set; }
    public int? PrerequisiteCourseId { get; set; }
}