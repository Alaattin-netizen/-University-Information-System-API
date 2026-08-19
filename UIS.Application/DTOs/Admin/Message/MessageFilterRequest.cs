namespace UIS.Application.DTOs.Filters;

public class MessageFilterRequest
{
    public int? SenderStudentId { get; set; }
    public int? ReceiverInstructorId { get; set; }
    public bool? IsRead { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}