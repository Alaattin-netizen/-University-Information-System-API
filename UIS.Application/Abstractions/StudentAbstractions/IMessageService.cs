using UIS.Application.DTOs.Student.Messages;

namespace UIS.Application.Abstractions.StudentAbstractions;

public interface IMessageService
{
    Task SendMessageAsync(int studentId, SendMessageRequest request);
}