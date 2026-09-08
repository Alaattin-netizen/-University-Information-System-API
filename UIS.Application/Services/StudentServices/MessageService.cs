using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.StudentAbstractions;
using UIS.Application.DTOs.Admin.Message;
using UIS.Application.DTOs.Student.Messages;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.StudentServices;

public class MessageService : IMessageService
{
    private readonly IUnitOfWork _unitOfWork;
 

    public MessageService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<IEnumerable<MessageResponse>> GetSentMessagesAsync(int studentId)
    {
        var messages = await _unitOfWork.Repository<Message>()
            .GetQueryable()
            .Include(m => m.Receiver)
            .Where(m => m.SenderStudentId == studentId)
            .OrderByDescending(m => m.SentDate)
            .ToListAsync();

        return messages.Select(m => new MessageResponse
        {
            Id = m.Id,
            ReceiverName = $"{m.Receiver.FirstName} {m.Receiver.LastName}",
            Subject = m.Subject,
            Content = m.Content,
            SentDate = m.SentDate,
            IsRead = m.IsRead
        });
    }

    public async Task<IEnumerable<UIS.Application.DTOs.Admin.Message.MessageResponse>> GetReceivedMessagesAsync(int instructorId)
    {
        var messages = await _unitOfWork.Repository<Message>()
            .GetQueryable()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.ReceiverInstructorId == instructorId)
            .OrderByDescending(m => m.SentDate)
            .ToListAsync();

        return messages.Select(m => new UIS.Application.DTOs.Admin.Message.MessageResponse
        {
            Id = m.Id,
            SenderStudentId = m.SenderStudentId,
            SenderName = $"{m.Sender.FirstName} {m.Sender.LastName}",
            SenderEmail = m.Sender.Email,
            ReceiverInstructorId = m.ReceiverInstructorId,
            ReceiverName = $"{m.Receiver.FirstName} {m.Receiver.LastName}",
            ReceiverEmail = m.Receiver.Email,
            Subject = m.Subject,
            Content = m.Content,
            SentDate = m.SentDate,
            IsRead = m.IsRead,
            ReadDate = m.ReadDate,
        });
    }
    public async Task SendMessageAsync(int studentId, SendMessageRequest request)
    {
        var studentRepo = _unitOfWork.Repository<User>();
        var student = await studentRepo.GetByIdAsync(studentId);

        if (student == null) throw new InvalidOperationException("Student not found.");

        var receiver = await _unitOfWork.Repository<User>()
        .GetQueryable()
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
        .FirstOrDefaultAsync(u => u.Id == request.ReceiverInstructorId);

        if (receiver == null)
            throw new InvalidOperationException("Receiver not found.");

        // ✅ 2. Verify the receiver has the "Instructor" role
        var isInstructor = receiver.UserRoles.Any(ur => ur.Role.Name == "Instructor");
        if (!isInstructor)
            throw new InvalidOperationException("You can only send messages to instructors.");

        var message = new Message
        {
            SenderStudentId = studentId,
            ReceiverInstructorId = request.ReceiverInstructorId,
            Subject = request.Subject,
            Content = request.Content,
            SentDate = DateTime.UtcNow,
            IsRead = false
        };

        var repo = _unitOfWork.Repository<Message>();
        await repo.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();
 
    }
}