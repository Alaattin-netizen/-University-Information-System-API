using UIS.Application.DTOs.Admin.Message;
using UIS.Application.DTOs.Filters;

namespace UIS.Application.Abstractions.AdminAbstractions;

public interface IAdminMessageService
{
    Task<MessageResponse> CreateAsync(CreateMessageRequest request);
    Task<MessageResponse> UpdateAsync(UpdateMessageRequest request);
    Task DeleteAsync(int id);
    Task<MessageResponse> GetByIdAsync(int id);
    Task<IEnumerable<MessageResponse>> GetAllAsync(MessageFilterRequest? filter=null);
    Task<IEnumerable<MessageResponse>> GetByStudentAsync(int studentId);
    Task<IEnumerable<MessageResponse>> GetByInstructorAsync(int instructorId);
}