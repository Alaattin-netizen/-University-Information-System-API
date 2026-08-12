namespace UIS.Application.DTOs.Admin.Message;

public class CreateMessageRequest
{
    public int SenderStudentId { get; set; }
    public int ReceiverInstructorId { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
}