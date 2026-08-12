using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin.AuditLog;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.AdminServices;

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // CREATE
    public async Task<AuditLogResponse> CreateAsync(CreateAuditLogRequest request)
    {
        var log = new AuditLog
        {
            UserId = request.UserId,
            UserEmail = request.UserEmail,
            UserRole = request.UserRole,
            Action = request.Action,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Details = request.Details,
            IpAddress = request.IpAddress,
            Timestamp = DateTime.UtcNow
        };

        await _unitOfWork.Repository<AuditLog>().AddAsync(log);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(log.Id);
    }

    // UPDATE
    public async Task<AuditLogResponse> UpdateAsync(UpdateAuditLogRequest request)
    {
        var log = await _unitOfWork.Repository<AuditLog>().GetByIdAsync(request.Id);
        if (log == null) throw new InvalidOperationException("Audit log not found.");

        if (!string.IsNullOrEmpty(request.Details))
            log.Details = request.Details;

        _unitOfWork.Repository<AuditLog>().Update(log);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(log.Id);
    }

    // DELETE
    public async Task DeleteAsync(int id)
    {
        var log = await _unitOfWork.Repository<AuditLog>().GetByIdAsync(id);
        if (log == null) throw new InvalidOperationException("Audit log not found.");

        _unitOfWork.Repository<AuditLog>().Delete(log);
        await _unitOfWork.SaveChangesAsync();
    }

    // GET BY ID
    public async Task<AuditLogResponse> GetByIdAsync(int id)
    {
        var log = await _unitOfWork.Repository<AuditLog>()
            .GetQueryable()
            .Include(al => al.User)
            .FirstOrDefaultAsync(al => al.Id == id);

        if (log == null) throw new InvalidOperationException("Audit log not found.");
        return MapToResponse(log);
    }

    // GET ALL
    public async Task<IEnumerable<AuditLogResponse>> GetAllAsync()
    {
        var logs = await _unitOfWork.Repository<AuditLog>()
            .GetQueryable()
            .Include(al => al.User)
            .OrderByDescending(al => al.Timestamp)
            .ToListAsync();

        return logs.Select(MapToResponse);
    }

    // GET BY USER ID
    public async Task<IEnumerable<AuditLogResponse>> GetByUserIdAsync(int userId)
    {
        var logs = await _unitOfWork.Repository<AuditLog>()
            .GetQueryable()
            .Include(al => al.User)
            .Where(al => al.UserId == userId)
            .OrderByDescending(al => al.Timestamp)
            .ToListAsync();

        return logs.Select(MapToResponse);
    }

    // Mapper
    private AuditLogResponse MapToResponse(AuditLog log) => new()
    {
        Id = log.Id,
        UserId = log.UserId,
        UserEmail = log.UserEmail,
        UserRole = log.UserRole,
        Action = log.Action,
        EntityType = log.EntityType,
        EntityId = log.EntityId,
        Details = log.Details,
        Timestamp = log.Timestamp,
        IpAddress = log.IpAddress
    };
}