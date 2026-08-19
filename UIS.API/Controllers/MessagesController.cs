using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin.Message;
using UIS.Application.DTOs.Filters;
using UIS.Application.Services;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize(Roles = "Admin")]
public class MessagesController : BaseApiController
{
    private readonly IAdminMessageService _messageService;
    private readonly LoggingHelper _loggingHelper;

    public MessagesController(IAdminMessageService messageService, LoggingHelper loggingHelper)
    {
        _messageService = messageService;
        _loggingHelper = loggingHelper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] MessageFilterRequest request)
        => Ok(await _messageService.GetAllAsync(request));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _messageService.GetByIdAsync(id));

    [HttpGet("student/{studentId}")]
    public async Task<IActionResult> GetByStudent(int studentId)
        => Ok(await _messageService.GetByStudentAsync(studentId));

    [HttpGet("instructor/{instructorId}")]
    public async Task<IActionResult> GetByInstructor(int instructorId)
        => Ok(await _messageService.GetByInstructorAsync(instructorId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMessageRequest request)
    {
        var result = await _messageService.CreateAsync(request);
        await _loggingHelper.LogOperationAsync("Created", "Message", result.Id, $"Subject: {request.Subject}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateMessageRequest request)
    {
        var result = await _messageService.UpdateAsync(request);
        await _loggingHelper.LogOperationAsync("Updated", "Message", result.Id, $"ID: {result.Id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _messageService.DeleteAsync(id);
        await _loggingHelper.LogOperationAsync("Deleted", "Message", id, $"ID: {id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return NoContent();
    }
}