using UIS.Application.DTOs.Admin.UserRole;

namespace UIS.Application.Abstractions.AdminAbstractions;

public interface IUserRoleService
{
    Task<UserRoleResponse> AssignRoleAsync(AssignRoleRequest request);

    Task RemoveRoleAsync(RemoveRoleRequest request);
    Task<UserRoleResponse> GetByIdAsync(int id);
    Task<IEnumerable<UserRoleResponse>> GetAllAsync();
    Task<IEnumerable<UserRoleResponse>> GetByUserIdAsync(int userId);
    Task<IEnumerable<UserRoleResponse>> GetByRoleIdAsync(int roleId);
}