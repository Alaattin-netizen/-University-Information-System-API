using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin.Message;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.AdminServices;

public class AdminMessageService : IAdminMessageService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminMessageService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<MessageResponse> CreateAsync(CreateMessageRequest request)
    {
        var sender = await _unitOfWork.Repository<User>().GetByIdAsync(request.SenderStudentId);
        if (sender == null) throw new InvalidOperationException("Sender not found.");
        var receiver = await _unitOfWork.Repository<User>().GetByIdAsync(request.ReceiverInstructorId);
        if (receiver == null) throw new InvalidOperationException("Receiver not found.");

        var message = new Message
        {
            SenderStudentId = request.SenderStudentId,
            ReceiverInstructorId = request.ReceiverInstructorId,
            Subject = request.Subject,
            Content = request.Content,
            SentDate = DateTime.UtcNow,
            IsRead = false
        };

        await _unitOfWork.Repository<Message>().AddAsync(message);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(message.Id);
    }

    public async Task<MessageResponse> UpdateAsync(UpdateMessageRequest request)
    {
        var message = await _unitOfWork.Repository<Message>()
            .GetQueryable()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .FirstOrDefaultAsync(m => m.Id == request.Id);

        if (message == null) throw new InvalidOperationException("Message not found.");

        if (request.IsRead.HasValue)
        {
            message.IsRead = request.IsRead.Value;
            if (request.IsRead.Value) message.ReadDate = DateTime.UtcNow;
        }
        if (!string.IsNullOrEmpty(request.Subject)) message.Subject = request.Subject;
        if (!string.IsNullOrEmpty(request.Content)) message.Content = request.Content;

        _unitOfWork.Repository<Message>().Update(message);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(message.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var message = await _unitOfWork.Repository<Message>().GetByIdAsync(id);
        if (message == null) throw new InvalidOperationException("Message not found.");
        _unitOfWork.Repository<Message>().Delete(message);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<MessageResponse> GetByIdAsync(int id)
    {
        var m = await _unitOfWork.Repository<Message>()
            .GetQueryable()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (m == null) throw new InvalidOperationException("Message not found.");
        return MapToResponse(m);
    }

    public async Task<IEnumerable<MessageResponse>> GetAllAsync()
    {
        var list = await _unitOfWork.Repository<Message>()
            .GetQueryable()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .OrderByDescending(m => m.SentDate)
            .ToListAsync();

        return list.Select(MapToResponse);
    }

    public async Task<IEnumerable<MessageResponse>> GetByStudentAsync(int studentId)
    {
        var list = await _unitOfWork.Repository<Message>()
            .GetQueryable()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderStudentId == studentId)
            .OrderByDescending(m => m.SentDate)
            .ToListAsync();

        return list.Select(MapToResponse);
    }

    public async Task<IEnumerable<MessageResponse>> GetByInstructorAsync(int instructorId)
    {
        var list = await _unitOfWork.Repository<Message>()
            .GetQueryable()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.ReceiverInstructorId == instructorId)
            .OrderByDescending(m => m.SentDate)
            .ToListAsync();

        return list.Select(MapToResponse);
    }

    private MessageResponse MapToResponse(Message m) => new()
    {
        Id = m.Id,
        SenderStudentId = m.SenderStudentId,
        SenderName = $"{m.Sender.FirstName} {m.Sender.LastName}",
        ReceiverInstructorId = m.ReceiverInstructorId,
        ReceiverName = $"{m.Receiver.FirstName} {m.Receiver.LastName}",
        Subject = m.Subject,
        Content = m.Content,
        SentDate = m.SentDate,
        IsRead = m.IsRead,
        ReadDate = m.ReadDate
    };
}