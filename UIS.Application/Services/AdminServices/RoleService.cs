using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.AdminServices;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<RoleResponse> CreateAsync(CreateRoleRequest request)
    {
        var existing = await _unitOfWork.Repository<Role>().GetFirstAsync(r => r.Name == request.Name);
        if (existing != null) throw new InvalidOperationException("Role with this name already exists.");

        var role = new Role { Name = request.Name, Description = request.Description };
        await _unitOfWork.Repository<Role>().AddAsync(role);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(role.Id);
    }

    public async Task<RoleResponse> UpdateAsync(UpdateRoleRequest request)
    {
        var role = await _unitOfWork.Repository<Role>().GetByIdAsync(request.Id);
        if (role == null) throw new InvalidOperationException("Role not found.");

        var duplicate = await _unitOfWork.Repository<Role>()
            .GetFirstAsync(r => r.Name == request.Name && r.Id != request.Id);
        if (duplicate != null) throw new InvalidOperationException("Another role with this name already exists.");

        role.Name = request.Name;
        role.Description = request.Description;

        _unitOfWork.Repository<Role>().Update(role);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(role.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var role = await _unitOfWork.Repository<Role>()
            .GetQueryable()
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null) throw new InvalidOperationException("Role not found.");
        if (role.UserRoles.Any()) throw new InvalidOperationException("Cannot delete role assigned to users.");

        _unitOfWork.Repository<Role>().Delete(role);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<RoleResponse> GetByIdAsync(int id)
    {
        var r = await _unitOfWork.Repository<Role>()
            .GetQueryable()
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (r == null) throw new InvalidOperationException("Role not found.");
        return MapToResponse(r);
    }

    public async Task<IEnumerable<RoleResponse>> GetAllAsync()
    {
        var list = await _unitOfWork.Repository<Role>()
            .GetQueryable()
            .Include(r => r.UserRoles)
            .ToListAsync();

        return list.Select(MapToResponse);
    }

    private RoleResponse MapToResponse(Role r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Description = r.Description,
        UserCount = r.UserRoles?.Count ?? 0
    };
}