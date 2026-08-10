using UIS.Application.DTOs.Messages;

namespace UIS.Application.Abstractions;

public interface IMessageService
{
    Task SendMessageAsync(int studentId, SendMessageRequest request);
}