using UIS.Application.DTOs.Admin.User;
using UIS.Application.DTOs.Filters;

public interface IUserService
{
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse> CreateStudentAsync(CreateStudentRequest request);
    Task<UserResponse> CreateInstructorAsync(CreateInstructorRequest request);
    Task<UserResponse> CreateAdminAsync(CreateAdminRequest request); // ✅ Added
    Task<UserResponse> AssignAdminRoleAsync(AssignAdminRoleRequest request);
    Task<IEnumerable<UserResponse>> GetAllUsersAsync(UserFilterRequest? filter=null);
    Task<UserResponse> GetUserByIdAsync(int id);
    Task DeleteUserAsync(int id);

    Task<UserResponse> UpdateUserAsync(UpdateUserRequest request);

}