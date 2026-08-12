using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.Abstractions;
using UIS.Application.DTOs.Auth;
using UIS.Application.Services;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly LoggingHelper _loggingHelper;
    public AuthController(IAuthService authService, LoggingHelper loggingHelper)
    {
        _authService = authService;
        _loggingHelper = loggingHelper;
    }

    [HttpPost("login")]
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

    [HttpPost("register")]
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