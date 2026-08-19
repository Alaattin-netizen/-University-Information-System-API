using UIS.Application.DTOs.Admin.UserRole;
using UIS.Application.DTOs.Filters;

namespace UIS.Application.Abstractions.AdminAbstractions;

public interface IUserRoleService
{
    Task<UserRoleResponse> AssignRoleAsync(AssignRoleRequest request);

    Task RemoveRoleAsync(RemoveRoleRequest request);
    Task<UserRoleResponse> GetByIdAsync(int id);
    Task<IEnumerable<UserRoleResponse>> GetAllAsync(UserRoleFilterRequest? filter=null);
    Task<IEnumerable<UserRoleResponse>> GetByUserIdAsync(int userId);
    Task<IEnumerable<UserRoleResponse>> GetByRoleIdAsync(int roleId);
}