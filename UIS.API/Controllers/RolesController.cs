using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Application.Services;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = "Admin")]
public class RolesController : BaseApiController
{
    private readonly IRoleService _roleService;
    private readonly LoggingHelper _loggingHelper;

    public RolesController(IRoleService roleService, LoggingHelper loggingHelper)
    {
        _roleService = roleService;
        _loggingHelper = loggingHelper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _roleService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _roleService.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var result = await _roleService.CreateAsync(request);
        await _loggingHelper.LogOperationAsync("Created", "Role", result.Id, $"Name: {request.Name}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateRoleRequest request)
    {
        var result = await _roleService.UpdateAsync(request);
        await _loggingHelper.LogOperationAsync("Updated", "Role", result.Id, $"Name: {request.Name}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _roleService.DeleteAsync(id);
        await _loggingHelper.LogOperationAsync("Deleted", "Role", id, $"ID: {id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return NoContent();
    }
}