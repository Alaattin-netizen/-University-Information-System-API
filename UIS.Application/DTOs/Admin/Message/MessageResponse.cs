namespace UIS.Application.DTOs.Admin.Message;

public class MessageResponse
{
    public int Id { get; set; }
    public int SenderStudentId { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    public int ReceiverInstructorId { get; set; }
    public string ReceiverName { get; set; }
    public string ReceiverEmail { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
    public DateTime SentDate { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadDate { get; set; }
}