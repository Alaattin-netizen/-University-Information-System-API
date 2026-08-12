using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin.User;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.AdminServices;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ======================================================
    // CREATE STUDENT
    // ======================================================

    public async Task<UserResponse> CreateStudentAsync(CreateStudentRequest request)
    {
        var existing = await _unitOfWork.Repository<User>()
            .GetFirstAsync(u => u.Email == request.Email);

        if (existing != null)
            throw new InvalidOperationException("Email already registered.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = passwordHash,
            DepartmentId = request.DepartmentId,
            AdvisorId = request.AdvisorId
        };

        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assign "Student" role
        var studentRole = await _unitOfWork.Repository<Role>()
            .GetFirstAsync(r => r.Name == "Student");

        if (studentRole != null)
        {
            var userRole = new UserRole { UserId = user.Id, RoleId = studentRole.Id };
            await _unitOfWork.Repository<UserRole>().AddAsync(userRole);
            await _unitOfWork.SaveChangesAsync();
        }

        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = new List<string> { "Student" },
            DepartmentId = user.DepartmentId,
            AdvisorId = user.AdvisorId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ======================================================
    // CREATE INSTRUCTOR
    // ======================================================

    public async Task<UserResponse> CreateInstructorAsync(CreateInstructorRequest request)
    {
        var existing = await _unitOfWork.Repository<User>()
            .GetFirstAsync(u => u.Email == request.Email);

        if (existing != null)
            throw new InvalidOperationException("Email already registered.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = passwordHash,
            DepartmentId = request.DepartmentId
        };

        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var instructorRole = await _unitOfWork.Repository<Role>()
            .GetFirstAsync(r => r.Name == "Instructor");

        if (instructorRole != null)
        {
            var userRole = new UserRole { UserId = user.Id, RoleId = instructorRole.Id };
            await _unitOfWork.Repository<UserRole>().AddAsync(userRole);
            await _unitOfWork.SaveChangesAsync();
        }

        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = new List<string> { "Instructor" },
            DepartmentId = user.DepartmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ======================================================
    // CREATE ADMIN
    // ======================================================

    public async Task<UserResponse> CreateAdminAsync(CreateAdminRequest request)
    {
        var existing = await _unitOfWork.Repository<User>()
            .GetFirstAsync(u => u.Email == request.Email);

        if (existing != null)
            throw new InvalidOperationException("Email already registered.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = passwordHash
        };

        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var adminRole = await _unitOfWork.Repository<Role>()
            .GetFirstAsync(r => r.Name == "Admin");

        if (adminRole != null)
        {
            var userRole = new UserRole { UserId = user.Id, RoleId = adminRole.Id };
            await _unitOfWork.Repository<UserRole>().AddAsync(userRole);
            await _unitOfWork.SaveChangesAsync();
        }

        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = new List<string> { "Admin" },
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ======================================================
    // ASSIGN ADMIN ROLE
    // ======================================================

    public async Task<UserResponse> AssignAdminRoleAsync(AssignAdminRoleRequest request)
    {
        var user = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId);

        if (user == null)
            throw new InvalidOperationException("User not found.");

        var hasAdminRole = user.UserRoles.Any(ur => ur.Role.Name == "Admin");
        if (hasAdminRole)
            throw new InvalidOperationException("User already has the Admin role.");

        var adminRole = await _unitOfWork.Repository<Role>()
            .GetFirstAsync(r => r.Name == "Admin");

        if (adminRole == null)
            throw new InvalidOperationException("Admin role not found.");

        var userRole = new UserRole { UserId = user.Id, RoleId = adminRole.Id };
        await _unitOfWork.Repository<UserRole>().AddAsync(userRole);
        await _unitOfWork.SaveChangesAsync();

        var updatedUser = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        return new UserResponse
        {
            Id = updatedUser.Id,
            FirstName = updatedUser.FirstName,
            LastName = updatedUser.LastName,
            Email = updatedUser.Email,
            Roles = updatedUser.UserRoles.Select(ur => ur.Role.Name).ToList(),
            DepartmentId = updatedUser.DepartmentId,
            AdvisorId = updatedUser.AdvisorId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ======================================================
    // GET ALL USERS
    // ======================================================

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Department)
            .Include(u => u.Advisor)
            .ToListAsync();

        return users.Select(u => new UserResponse
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            DepartmentId = u.DepartmentId,
            
            AdvisorId = u.AdvisorId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    }

    // ======================================================
    // GET USER BY ID
    // ======================================================

    public async Task<UserResponse> GetUserByIdAsync(int id)
    {
        var user = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Department)
            .Include(u => u.Advisor)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            throw new InvalidOperationException("User not found.");

        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            DepartmentId = user.DepartmentId,
            AdvisorId = user.AdvisorId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ======================================================
    // DELETE USER
    // ======================================================

    public async Task DeleteUserAsync(int id)
    {
        var user = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            throw new InvalidOperationException("User not found.");

        // Check if user is an instructor with course offerings
        var hasOfferings = await _unitOfWork.Repository<CourseOffering>()
            .GetQueryable()
            .AnyAsync(o => o.InstructorId == id);

        if (hasOfferings)
            throw new InvalidOperationException("Cannot delete user with active course offerings.");

        // Remove roles first (manual cascade)
        if (user.UserRoles.Any())
        {
            _unitOfWork.Repository<UserRole>().DeleteRange(user.UserRoles);
        }

        _unitOfWork.Repository<User>().Delete(user);
        await _unitOfWork.SaveChangesAsync();
    }
}