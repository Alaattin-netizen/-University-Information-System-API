using Microsoft.AspNetCore.Http;
using UIS.Domain.Entities;          // ✅ Add this for AuditLog
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services
{
    public class Helper
    {
        public enum Role
        {
            Student = 1,
            Instructor = 2,
            Admin = 3
        }

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public Helper(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        public async Task LogOperationAsync(string action, string entityType, int? entityId, string? details = null)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("nameid")?.Value;
            var userId = int.TryParse(userIdClaim, out var id) ? id : 0;

            var userEmail = _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value ?? "Unknown";
            var userRole = _httpContextAccessor.HttpContext?.User?.FindFirst("role")?.Value ?? "Unknown";
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

            var log = new AuditLog
            {
                UserId = userId,
                UserEmail = userEmail,
                UserRole = userRole,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress
            };

            await _unitOfWork.Repository<AuditLog>().AddAsync(log);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}