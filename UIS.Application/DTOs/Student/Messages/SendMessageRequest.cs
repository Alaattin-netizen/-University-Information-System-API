namespace UIS.Application.DTOs.Student.Messages;

public class SendMessageRequest
{

    public int ReceiverInstructorId { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
}