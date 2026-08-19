using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.DTOs.Admin.Course;
using UIS.Application.DTOs.Filters;
using UIS.Application.Services;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : BaseApiController
{
    private readonly IFacultyService _facultyService;
    private readonly LoggingHelper _loggingHelper;

    public CoursesController(IFacultyService facultyService, LoggingHelper loggingHelper)
    {
        _facultyService = facultyService;
        _loggingHelper = loggingHelper;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] CourseFilterRequest filter)
        => Ok(await _facultyService.GetAllCoursesAsync(filter));

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _facultyService.GetCourseByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        var result = await _facultyService.CreateCourseAsync(request);
        await _loggingHelper.LogOperationAsync("Created", "Course", result.Id, $"Code: {request.Code}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateCourseRequest request)
    {
        var result = await _facultyService.UpdateCourseAsync(request);
        await _loggingHelper.LogOperationAsync("Updated", "Course", result.Id, $"Code: {request.Code}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _facultyService.DeleteCourseAsync(id);
        await _loggingHelper.LogOperationAsync("Deleted", "Course", id, $"ID: {id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return NoContent();
    }
}