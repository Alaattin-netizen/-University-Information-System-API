using UIS.Application.DTOs.Admin.User;

public interface IUserService
{
    Task<UserResponse> CreateStudentAsync(CreateStudentRequest request);
    Task<UserResponse> CreateInstructorAsync(CreateInstructorRequest request);
    Task<UserResponse> CreateAdminAsync(CreateAdminRequest request); // ✅ Added
    Task<UserResponse> AssignAdminRoleAsync(AssignAdminRoleRequest request);
    Task<IEnumerable<UserResponse>> GetAllUsersAsync();
    Task<UserResponse> GetUserByIdAsync(int id);
    Task DeleteUserAsync(int id);

}