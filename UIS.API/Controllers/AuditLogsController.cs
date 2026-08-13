using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin.AuditLog;
using UIS.Application.Services;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : BaseApiController
{
    private readonly IAuditLogService _auditLogService;
    private readonly LoggingHelper _loggingHelper;

    public AuditLogsController(IAuditLogService auditLogService, LoggingHelper loggingHelper)
    {
        _auditLogService = auditLogService;
        _loggingHelper = loggingHelper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _auditLogService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _auditLogService.GetByIdAsync(id));

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(int userId)
        => Ok(await _auditLogService.GetByUserIdAsync(userId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAuditLogRequest request)
    {
        var result = await _auditLogService.CreateAsync(request);
        await _loggingHelper.LogOperationAsync("Created", "AuditLog", result.Id, $"Action: {request.Action}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateAuditLogRequest request)
    {
        var result = await _auditLogService.UpdateAsync(request);
        await _loggingHelper.LogOperationAsync("Updated", "AuditLog", result.Id, $"ID: {result.Id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _auditLogService.DeleteAsync(id);
        await _loggingHelper.LogOperationAsync("Deleted", "AuditLog", id, $"ID: {id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return NoContent();
    }
}