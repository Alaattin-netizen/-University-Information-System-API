using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UIS.Application.Abstractions;
using UIS.Application.DTOs.Auth;
using UIS.Application.Services;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly LoggingHelper _loggingHelper;
    private readonly IUnitOfWork _unitOfWork;
    public AuthController(IAuthService authService, LoggingHelper loggingHelper, IUnitOfWork unitOfWork)
    {
        _authService = authService;
        _loggingHelper = loggingHelper;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);

            // ✅ Log success using the result data (not HttpContext.User)
            await _loggingHelper.LogOperationAsync(
                "LoggedIn",
                "User",
                result.UserId,
                $"Logged in",
                result.UserId,
                result.Email,
                result.Roles // List<string>
            );

            SetAuthCookie(result.Token, result.ExpiresAt);
            return Ok(result);
        }

        catch (UnauthorizedAccessException ex)
        {
            // Log failed login attempt
            await _loggingHelper.LogOperationAsync(
                "LoginFailed",
                "User",
                null,
                $"Email: {request.Email}",
                0,
                "Unknown",
                new List<string>() // no roles
            );

            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (userId is null || email is null)
            return Unauthorized();

        var user = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .FirstOrDefaultAsync(user => user.Id == int.Parse(userId));
        if (user is null)
            return Unauthorized();

        return Ok(new AuthResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToList(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
        });
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("uis_auth", new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Path = "/",
        });
        return NoContent();
    }

    private void SetAuthCookie(string token, DateTime expiresAt)
    {
        Response.Cookies.Append("uis_auth", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = expiresAt,
            Path = "/",
        });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            await _loggingHelper.LogOperationAsync(
                "LoggedIn",
                "User",
                result.UserId,
                $"Registered",
                result.UserId,
                result.Email,
                result.Roles // List<string>
            );
            SetAuthCookie(result.Token, result.ExpiresAt);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            // Log failed registration attempt
            await _loggingHelper.LogOperationAsync(
                "RegisterFailed",
                "User",
                null,
                $"Email: {request.Email}",
                0,
                "Unknown",
                new List<string>() // no roles
            );
            return BadRequest(new { message = ex.Message });
        }
    }

}