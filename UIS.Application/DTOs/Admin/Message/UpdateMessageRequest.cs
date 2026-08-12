namespace UIS.Application.DTOs.Admin.Message;

public class UpdateMessageRequest
{
    public int Id { get; set; }
    public bool? IsRead { get; set; }
    public string? Subject { get; set; }
    public string? Content { get; set; }
}