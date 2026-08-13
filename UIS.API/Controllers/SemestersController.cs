using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.DTOs.Admin.Semester;
using UIS.Application.Services;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/semesters")]
public class SemestersController : BaseApiController
{
    private readonly ISemesterService _semesterService;
    private readonly LoggingHelper _loggingHelper;

    public SemestersController(ISemesterService semesterService, LoggingHelper loggingHelper)
    {
        _semesterService = semesterService;
        _loggingHelper = loggingHelper;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
        => Ok(await _semesterService.GetAllSemestersAsync());

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _semesterService.GetSemesterByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateSemesterRequest request)
    {
        var result = await _semesterService.CreateSemesterAsync(request);
        await _loggingHelper.LogOperationAsync("Created", "Semester", result.Id, $"Name: {request.Name}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateSemesterRequest request)
    {
        var result = await _semesterService.UpdateSemesterAsync(request);
        await _loggingHelper.LogOperationAsync("Updated", "Semester", result.Id, $"Name: {request.Name}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _semesterService.DeleteSemesterAsync(id);
        await _loggingHelper.LogOperationAsync("Deleted", "Semester", id, $"ID: {id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return NoContent();
    }

    [HttpPut("{semesterId}/calendar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRegistrationCalendar(int semesterId, [FromBody] UpdateRegistrationDateRequest request)
    {
        var result = await _semesterService.UpdateRegistrationCalendarAsync(semesterId, request);
        await _loggingHelper.LogOperationAsync("Updated", "Semester", semesterId, $"Calendar updated for Semester ID: {semesterId}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return Ok(result);
    }
}