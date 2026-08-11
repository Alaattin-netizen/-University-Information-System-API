using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions;
using UIS.Infrastructure.Repositories;
using UIS.Application.DTOs.Auth;
using UIS.Domain.Entities;
using UIS.Domain.Entities.Users;
using UIS.Domain.Enums;
using UIS.Application.Abstractions.StudentAbstractions;

namespace UIS.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly LoggingHelper _loggingHelper;

    public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, LoggingHelper loggingHelper)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _loggingHelper = loggingHelper;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _unitOfWork.Repository<User>()
            .GetFirstAsync(u => u.Email == request.Email);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // ✅ FIXED: Verify the hashed password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role.ToString());

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if user already exists
        var existingUser = await _unitOfWork.Repository<User>()
            .GetFirstAsync(u => u.Email == request.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email already registered");
        }

        // Create user based on role
        User user = request.Role switch
        {
            Role.Student => new Student
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = Role.Student,
                DepartmentId = request.DepartmentId
            },
            Role.Instructor => new Instructor
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = Role.Instructor,
                DepartmentId = request.DepartmentId
            },
            Role.Admin => new Admin
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = Role.Admin
            },
            _ => throw new InvalidOperationException("Invalid role")
        };

        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        await _loggingHelper.LogOperationAsync(
           "Created",
           "User",
           user.Id,
           $"Email: {user.Email}, Role: {user.Role}"
       );
        var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role.ToString());

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }
}