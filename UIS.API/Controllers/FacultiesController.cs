using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.DTOs.Admin.Faculty;
using UIS.Application.DTOs.Filters;
using UIS.Application.Services;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/faculties")]
public class FacultiesController : BaseApiController
{
    private readonly IFacultyService _facultyService;
    private readonly LoggingHelper _loggingHelper;

    public FacultiesController(IFacultyService facultyService, LoggingHelper loggingHelper)
    {
        _facultyService = facultyService;
        _loggingHelper = loggingHelper;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] FacultyFilterRequest filter)
        => Ok(await _facultyService.GetAllFacultiesAsync(filter));

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _facultyService.GetFacultyByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateFacultyRequest request)
    {
        var result = await _facultyService.CreateFacultyAsync(request);
        await _loggingHelper.LogOperationAsync("Created", "Faculty", result.Id, $"Name: {request.Name}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateFacultyRequest request)
    {
        var result = await _facultyService.UpdateFacultyAsync(request);
        await _loggingHelper.LogOperationAsync("Updated", "Faculty", result.Id, $"Name: {request.Name}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _facultyService.DeleteFacultyAsync(id);
        await _loggingHelper.LogOperationAsync("Deleted", "Faculty", id, $"ID: {id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return NoContent();
    }
}