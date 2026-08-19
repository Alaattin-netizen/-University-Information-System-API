using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Application.DTOs.Filters;
using UIS.Application.Services;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/announcements")]
public class AnnouncementsController : BaseApiController
{
    private readonly IAnnouncementService _announcementService;
    private readonly LoggingHelper _loggingHelper;

    public AnnouncementsController(IAnnouncementService announcementService, LoggingHelper loggingHelper)
    {
        _announcementService = announcementService;
        _loggingHelper = loggingHelper;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] AnnouncementFilterRequest request)
        => Ok(await _announcementService.GetAllAsync(request));

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _announcementService.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] AdminCreateAnnouncementRequest request)
    {
        var result = await _announcementService.CreateAsync(request);
        await _loggingHelper.LogOperationAsync("Created", "Announcement", result.Id, $"Title: {request.Title}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateAnnouncementRequest request)
    {
        var result = await _announcementService.UpdateAsync(request);
        await _loggingHelper.LogOperationAsync("Updated", "Announcement", result.Id, $"Title: {request.Title}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _announcementService.DeleteAsync(id);
        await _loggingHelper.LogOperationAsync("Deleted", "Announcement", id, $"ID: {id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return NoContent();
    }
}