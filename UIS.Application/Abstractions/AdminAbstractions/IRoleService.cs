using UIS.Application.DTOs.Admin;

namespace UIS.Application.Abstractions.AdminAbstractions;

public interface IRoleService
{
    Task<RoleResponse> CreateAsync(CreateRoleRequest request);
    Task<RoleResponse> UpdateAsync(UpdateRoleRequest request);
    Task DeleteAsync(int id);
    Task<RoleResponse> GetByIdAsync(int id);
    Task<IEnumerable<RoleResponse>> GetAllAsync();
}