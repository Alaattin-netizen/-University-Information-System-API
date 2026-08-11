using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.AdminServices;

public class LogService : ILogService
{
    private readonly IUnitOfWork _unitOfWork;

    public LogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<UserOperationResponse>> GetLogsAsync(int userId)
    {
        var logs = await _unitOfWork.Repository<AuditLog>()
            .GetQueryable()
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();

        return logs.Select(l => new UserOperationResponse
        {
            Id = l.Id,
            UserId = l.UserId,
            UserEmail = l.UserEmail,
            UserRole = l.UserRole,
            Action = l.Action,
            EntityType = l.EntityType,
            EntityId = l.EntityId,
            Details = l.Details,
            Timestamp = l.Timestamp,
            IpAddress = l.IpAddress
        });
    }

    public async Task<IEnumerable<UserOperationResponse>> GetAllLogsAsync()
    {
        var logs = await _unitOfWork.Repository<AuditLog>()
            .GetQueryable()
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();

        return logs.Select(l => new UserOperationResponse
        {
            Id = l.Id,
            UserId = l.UserId,
            UserEmail = l.UserEmail,
            UserRole = l.UserRole,
            Action = l.Action,
            EntityType = l.EntityType,
            EntityId = l.EntityId,
            Details = l.Details,
            Timestamp = l.Timestamp,
            IpAddress = l.IpAddress
        });
    }
}