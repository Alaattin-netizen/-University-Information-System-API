using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services;

public class LoggingHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;

    public LoggingHelper(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
    }

    public async Task LogOperationAsync(
     string action,
     string entityType,
     int? entityId,
     string? details = null,
     int userId = 0,
     string userEmail = "Unknown",
     string userRole = "Unknown")
    {
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
            IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown"
        };

        await _unitOfWork.Repository<AuditLog>().AddAsync(log);
        await _unitOfWork.SaveChangesAsync();
    }
}