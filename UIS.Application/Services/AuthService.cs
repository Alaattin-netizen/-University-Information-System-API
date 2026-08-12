using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions;
using UIS.Application.DTOs.Auth;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

        var token = _jwtService.GenerateToken(user.Id, user.Email, roles);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Roles = roles,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Validate email
        var existingUser = await _unitOfWork.Repository<User>()
            .GetFirstAsync(u => u.Email == request.Email);

        if (existingUser != null)
            throw new InvalidOperationException("Email already registered.");

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create user
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

        // Assign roles
        if (request.Roles.Any())
        {
            var roleNames = request.Roles.Select(r => r.Trim()).ToList();
            var roles = await _unitOfWork.Repository<Role>()
                .GetQueryable()
                .Where(r => roleNames.Contains(r.Name))
                .ToListAsync();

            foreach (var role in roles)
            {
                var userRole = new UserRole { UserId = user.Id, RoleId = role.Id };
                await _unitOfWork.Repository<UserRole>().AddAsync(userRole);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        var userRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var token = _jwtService.GenerateToken(user.Id, user.Email, userRoles);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Roles = userRoles,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }
}