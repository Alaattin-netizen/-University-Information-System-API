using UIS.Application.Abstractions.StudentAbstractions;
using UIS.Application.DTOs.Student.Messages;
using UIS.Domain.Entities;
using UIS.Domain.Entities.Users;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.StudentServices;

public class MessageService : IMessageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly LoggingHelper _loggingHelper;  

    public MessageService(IUnitOfWork unitOfWork, LoggingHelper loggingHelper)
    {
        _unitOfWork = unitOfWork;
        _loggingHelper = loggingHelper;
    }

    public async Task SendMessageAsync(int studentId, SendMessageRequest request)
    {
        var studentRepo = _unitOfWork.Repository<Student>();
        var student = await studentRepo.GetByIdAsync(studentId);

        if (student == null) throw new Exception("Student not found.");
        if (student.AdvisorId == null) throw new Exception("Student has no assigned advisor.");

        var message = new Message
        {
            SenderStudentId = studentId,
            ReceiverInstructorId = student.AdvisorId.Value,
            Subject = request.Subject,
            Content = request.Content,
            SentDate = DateTime.UtcNow,
            IsRead = false
        };

        var repo = _unitOfWork.Repository<Message>();
        await repo.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();
        await _loggingHelper.LogOperationAsync(
           "Created",
           "Message",
           message.Id,
           $"From: {studentId}, To: {student.AdvisorId.Value}"
       );
    }
}