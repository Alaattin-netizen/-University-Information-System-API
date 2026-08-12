using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin.UserRole;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.AdminServices;

public class UserRoleService : IUserRoleService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserRoleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // CREATE
    public async Task<UserRoleResponse> AssignRoleAsync(AssignRoleRequest request)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId);
        if (user == null) throw new InvalidOperationException("User not found.");

        var role = await _unitOfWork.Repository<Role>().GetByIdAsync(request.RoleId);
        if (role == null) throw new InvalidOperationException("Role not found.");

        var existing = await _unitOfWork.Repository<UserRole>()
            .GetFirstAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId);
        if (existing != null) throw new InvalidOperationException("User already has this role.");

        var userRole = new UserRole { UserId = request.UserId, RoleId = request.RoleId };
        await _unitOfWork.Repository<UserRole>().AddAsync(userRole);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(userRole.Id);
    }

    // DELETE
    public async Task RemoveRoleAsync(RemoveRoleRequest request)
    {
        var userRole = await _unitOfWork.Repository<UserRole>()
            .GetFirstAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId);

        if (userRole == null) throw new InvalidOperationException("User does not have this role.");

        _unitOfWork.Repository<UserRole>().Delete(userRole);
        await _unitOfWork.SaveChangesAsync();
    }

    // GET BY ID
    public async Task<UserRoleResponse> GetByIdAsync(int id)
    {
        var userRole = await _unitOfWork.Repository<UserRole>()
            .GetQueryable()
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.Id == id);

        if (userRole == null) throw new InvalidOperationException("UserRole assignment not found.");
        return MapToResponse(userRole);
    }

    // GET ALL
    public async Task<IEnumerable<UserRoleResponse>> GetAllAsync()
    {
        var userRoles = await _unitOfWork.Repository<UserRole>()
            .GetQueryable()
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .ToListAsync();

        return userRoles.Select(MapToResponse);
    }

    // GET BY USER ID
    public async Task<IEnumerable<UserRoleResponse>> GetByUserIdAsync(int userId)
    {
        var userRoles = await _unitOfWork.Repository<UserRole>()
            .GetQueryable()
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

        return userRoles.Select(MapToResponse);
    }

    // GET BY ROLE ID
    public async Task<IEnumerable<UserRoleResponse>> GetByRoleIdAsync(int roleId)
    {
        var userRoles = await _unitOfWork.Repository<UserRole>()
            .GetQueryable()
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .Where(ur => ur.RoleId == roleId)
            .ToListAsync();

        return userRoles.Select(MapToResponse);
    }

    // Mapper
    private UserRoleResponse MapToResponse(UserRole ur) => new()
    {
        Id = ur.Id,
        UserId = ur.UserId,
        RoleId = ur.RoleId
    };
}