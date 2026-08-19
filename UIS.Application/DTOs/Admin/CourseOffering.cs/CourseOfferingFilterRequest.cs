namespace UIS.Application.DTOs.Filters;

public class CourseOfferingFilterRequest
{
    public int? SemesterId { get; set; }
    public int? InstructorId { get; set; }
    public int? CourseId { get; set; }



    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public string Day { get; set; }

    public int EnrolledCount { get; set; }


}