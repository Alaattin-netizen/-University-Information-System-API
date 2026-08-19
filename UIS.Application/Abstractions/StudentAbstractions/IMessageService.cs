using UIS.Application.DTOs.Student.Messages;
using UIS.Application.DTOs.Admin.Message;
namespace UIS.Application.Abstractions.StudentAbstractions;

public interface IMessageService
{
    Task SendMessageAsync(int studentId,  SendMessageRequest request);
    Task<IEnumerable<MessageResponse>> GetSentMessagesAsync(int studentId);
}