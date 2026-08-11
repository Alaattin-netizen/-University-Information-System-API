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
            // Log success
            await _loggingHelper.LogOperationAsync(
                "LoggedIn",
                "User",
                result.UserId,
                $"Email: {result.Email}, Role: {result.Role}",
                result.UserId, // user is now authenticated
                result.Email,
                result.Role.ToString()
            );
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Optionally log failed login attempts (without user info)
            await _loggingHelper.LogOperationAsync(
                "LoginFailed",
                "User",
                null,
                $"Email: {request.Email}",
                0,
                "Unknown",
                "Unknown"
            );
            return Unauthorized(new { message = ex.Message });
        }
    }

    //[HttpPost("register")]
    //public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    //{
    //    try
    //    {
    //        var result = await _authService.RegisterAsync(request);
    //        return Ok(result);
    //    }
    //    catch (InvalidOperationException ex)
    //    {
    //        return BadRequest(new { message = ex.Message });
    //    }
    //}

}