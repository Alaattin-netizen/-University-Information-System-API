using UIS.Application.DTOs.Admin.Message;

namespace UIS.Application.Abstractions.AdminAbstractions;

public interface IAdminMessageService
{
    Task<MessageResponse> CreateAsync(CreateMessageRequest request);
    Task<MessageResponse> UpdateAsync(UpdateMessageRequest request);
    Task DeleteAsync(int id);
    Task<MessageResponse> GetByIdAsync(int id);
    Task<IEnumerable<MessageResponse>> GetAllAsync();
    Task<IEnumerable<MessageResponse>> GetByStudentAsync(int studentId);
    Task<IEnumerable<MessageResponse>> GetByInstructorAsync(int instructorId);
}