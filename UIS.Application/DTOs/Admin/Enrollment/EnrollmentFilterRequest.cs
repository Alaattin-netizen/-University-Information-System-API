namespace UIS.Application.DTOs.Admin.Enrollment;

public class EnrollmentFilterRequest
{
    public int? StudentId { get; set; }
    public int? CourseOfferingId { get; set; }
    public bool? IsActive { get; set; }
}