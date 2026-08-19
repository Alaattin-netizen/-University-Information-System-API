using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin.UserRole;
using UIS.Application.DTOs.Filters;
using UIS.Application.Services;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/user-roles")]
[Authorize(Roles = "Admin")]
public class UserRolesController : BaseApiController
{
    private readonly IUserRoleService _userRoleService;
    private readonly LoggingHelper _loggingHelper;

    public UserRolesController(IUserRoleService userRoleService, LoggingHelper loggingHelper)
    {
        _userRoleService = userRoleService;
        _loggingHelper = loggingHelper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] UserRoleFilterRequest filter)
        => Ok(await _userRoleService.GetAllAsync(filter));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _userRoleService.GetByIdAsync(id));

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(int userId)
        => Ok(await _userRoleService.GetByUserIdAsync(userId));

    [HttpGet("role/{roleId}")]
    public async Task<IActionResult> GetByRole(int roleId)
        => Ok(await _userRoleService.GetByRoleIdAsync(roleId));

    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] AssignRoleRequest request)
    {
        var result = await _userRoleService.AssignRoleAsync(request);
        await _loggingHelper.LogOperationAsync("Assigned", "UserRole", result.Id, $"User: {request.UserId}, Role: {request.RoleId}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> Remove([FromBody] RemoveRoleRequest request)
    {
        await _userRoleService.RemoveRoleAsync(request);
        await _loggingHelper.LogOperationAsync("Removed", "UserRole", null, $"User: {request.UserId}, Role: {request.RoleId}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return NoContent();
    }
}