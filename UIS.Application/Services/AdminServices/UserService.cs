using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Domain.Entities.Users;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.AdminServices
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserCreationService _userCreationService;
        private readonly Helper _helper; // ✅ Inject Helper

        public UserService(IUnitOfWork unitOfWork, UserCreationService userCreationService, Helper helper)
        {
            _unitOfWork = unitOfWork;
            _userCreationService = userCreationService;
            _helper = helper; // ✅
        }


        public async Task<UserResponse> CreateStudentAsync(CreateStudentRequest request)
        {
            // 1. Validate email
            var existingUser = await _unitOfWork.Repository<User>()
                .GetFirstAsync(u => u.Email == request.Email);

            if (existingUser != null)
                throw new InvalidOperationException("Email already registered.");

            // 2. Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            // 3. Create user via Infrastructure (returns only the ID)
            var userId = await _userCreationService.CreateStudentAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                passwordHash,
                request.DepartmentId,
                request.AdvisorId);

            // 4. Log
            await _helper.LogOperationAsync("Created", "Student", userId, $"Email: {request.Email}");

            // 5. Return response (Application builds DTO, no Domain reference)
            return new UserResponse
            {
                Id = userId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Role = "Student",
                DepartmentId = request.DepartmentId,
                AdvisorId = request.AdvisorId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<UserResponse> CreateInstructorAsync(CreateInstructorRequest request)
        {
            // 1. Validate email
            var existingUser = await _unitOfWork.Repository<User>()
                .GetFirstAsync(u => u.Email == request.Email);

            if (existingUser != null)
                throw new InvalidOperationException("Email already registered.");

            // 2. Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            // 3. Create user via Infrastructure (returns only the ID)
            var userId = await _userCreationService.CreateInstructorAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                passwordHash,
                request.DepartmentId);

            // 4. Log
            await _helper.LogOperationAsync("Created", "Instructor", userId, $"Email: {request.Email}");

            // 5. Return response (Application builds DTO, no Domain reference)
            return new UserResponse
            {
                Id = userId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Role = "Instructor",
                DepartmentId = request.DepartmentId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

    }

}
        



