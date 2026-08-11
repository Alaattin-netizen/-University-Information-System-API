using BCrypt.Net;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Domain.Entities;          // ✅ YES, Application CAN reference Domain
using UIS.Domain.Entities.Users;    // ✅ YES
using UIS.Domain.Enums;             // ✅ YES
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.AdminServices;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;         // ✅ Direct access to DB
    private readonly LoggingHelper _loggingHelper;

    public UserService(IUnitOfWork unitOfWork, LoggingHelper loggingHelper)
    {
        _unitOfWork = unitOfWork;
        _loggingHelper = loggingHelper;
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

        // 3. Create Domain entity (THIS IS FINE!)
        var user = new Student
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = passwordHash,
            Role = Role.Student,
            DepartmentId = request.DepartmentId,
            AdvisorId = request.AdvisorId
        };

        // 4. Save
        await _unitOfWork.Repository<Student>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // 5. Log
        await _loggingHelper.LogOperationAsync("Created", "Student", user.Id, $"Email: {user.Email}");

        // 6. Return DTO
        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = "Student",
            DepartmentId = user.DepartmentId,
            AdvisorId = (user as Student)?.AdvisorId,
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

        // 3. Create Domain entity (THIS IS FINE!)
        var user = new Student
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = passwordHash,
            Role = Role.Instructor,
            DepartmentId = request.DepartmentId
           
        };

        // 4. Save
        await _unitOfWork.Repository<Student>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // 5. Log
        await _loggingHelper.LogOperationAsync("Created", "Instructor", user.Id, $"Email: {user.Email}");

        // 6. Return DTO
        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = "Instructor",
            DepartmentId = user.DepartmentId,
            AdvisorId = (user as Student)?.AdvisorId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

}