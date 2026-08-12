using UIS.Application.DTOs.Admin.AuditLog;

namespace UIS.Application.Abstractions.AdminAbstractions;

public interface IAuditLogService
{
    Task<AuditLogResponse> CreateAsync(CreateAuditLogRequest request);
    Task<AuditLogResponse> UpdateAsync(UpdateAuditLogRequest request);
    Task DeleteAsync(int id);
    Task<AuditLogResponse> GetByIdAsync(int id);
    Task<IEnumerable<AuditLogResponse>> GetAllAsync();
    Task<IEnumerable<AuditLogResponse>> GetByUserIdAsync(int userId);
}