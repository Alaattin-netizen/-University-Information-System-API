using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.DTOs.Admin.User;
using UIS.Application.DTOs.Filters;
using UIS.Application.DTOs.Profile;
using UIS.Application.Abstractions;
using UIS.Application.Services;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly IProfileService _profileService;
    private readonly LoggingHelper _loggingHelper;

    public UserController(IUserService userService, IProfileService profileService, LoggingHelper loggingHelper)
    {
        _userService = userService;
        _loggingHelper = loggingHelper;
        _profileService = profileService;
    }

    // ============================================================
    // GET ALL USERS (Admin only)
    // ============================================================
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers([FromQuery] UserFilterRequest? filter)
    {
        var result = await _userService.GetAllUsersAsync(filter);
        return Ok(result);
    }

    // ============================================================
    // GET USER BY ID (Admin only)
    // ============================================================
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return Ok(result);
    }

    // ============================================================
    // GET CURRENT USER (Authenticated users)
    // ============================================================
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        var result = await _userService.GetUserByIdAsync(userId);
        return Ok(result);
    }

    // ============================================================
    // CREATE STUDENT (Admin only)
    // ============================================================
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var result = await _userService.CreateUserAsync(request);
            await _loggingHelper.LogOperationAsync(
                "Created",
                "User",
                result.Id,
                $"Email: {request.Email}; Roles: {string.Join(", ", result.Roles)}",
                GetCurrentUserId(),
                GetCurrentUserEmail(),
                GetCurrentUserRoles()
            );
            return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("student")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
    {
        var result = await _userService.CreateStudentAsync(request);
        await _loggingHelper.LogOperationAsync(
            "Created",
            "Student",
            result.Id,
            $"Email: {request.Email}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
    }

    // ============================================================
    // CREATE INSTRUCTOR (Admin only)
    // ============================================================
    [HttpPost("instructor")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateInstructor([FromBody] CreateInstructorRequest request)
    {
        var result = await _userService.CreateInstructorAsync(request);
        await _loggingHelper.LogOperationAsync(
            "Created",
            "Instructor",
            result.Id,
            $"Email: {request.Email}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
    }

    // ============================================================
    // CREATE ADMIN (Admin only)
    // ============================================================
    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest request)
    {
        var result = await _userService.CreateAdminAsync(request);
        await _loggingHelper.LogOperationAsync(
            "Created",
            "Admin",
            result.Id,
            $"Email: {request.Email}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
    }

    // ============================================================
    // UPDATE USER (Admin only)
    // ============================================================
    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
    {
        var result = await _userService.UpdateUserAsync(request);
        await _loggingHelper.LogOperationAsync(
            "Updated",
            "User",
            result.Id,
            $"Email: {result.Email}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return Ok(result);
    }

    // ============================================================
    // DELETE USER (Admin only)
    // ============================================================
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteUserAsync(id);
        await _loggingHelper.LogOperationAsync(
            "Deleted",
            "User",
            id,
            $"User ID: {id}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return NoContent();
    }

    // ============================================================
    // ASSIGN ADMIN ROLE (Admin only)
    // ============================================================
    [HttpPost("assign-admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignAdminRole([FromBody] AssignAdminRoleRequest request)
    {
        var result = await _userService.AssignAdminRoleAsync(request);
        await _loggingHelper.LogOperationAsync(
            "AssignedRole",
            "User",
            result.Id,
            $"User {result.Email} granted Admin role",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return Ok(result);
    }

    [HttpPut("me/profile")]
    [Authorize] // ✅ All authenticated users (any role)
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
    {
        await _profileService.UpdateProfileAsync(GetCurrentUserId(), request);
        await _loggingHelper.LogOperationAsync(
        "Updated",
        "Profile",
        GetCurrentUserId(),
        $"New Name: {request.FirstName} {request.LastName}",
        GetCurrentUserId(),
        GetCurrentUserEmail(),
        GetCurrentUserRoles()
    );
        return Ok(new { message = "Profile updated successfully." });
    }
}