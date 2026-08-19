using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Application.DTOs.Filters;
using UIS.Application.Services;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/course-offerings")]
public class CourseOfferingsController : BaseApiController
{
    private readonly ICourseOfferingService _courseOfferingService;
    private readonly LoggingHelper _loggingHelper;

    public CourseOfferingsController(ICourseOfferingService courseOfferingService, LoggingHelper loggingHelper)
    {
        _courseOfferingService = courseOfferingService;
        _loggingHelper = loggingHelper;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] CourseOfferingFilterRequest filter)
        => Ok(await _courseOfferingService.GetAllAsync(filter));

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _courseOfferingService.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCourseOfferingRequest request)
    {
        var result = await _courseOfferingService.CreateAsync(request);
        await _loggingHelper.LogOperationAsync("Created", "CourseOffering", result.Id, $"Course: {result.CourseCode}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateCourseOfferingRequest request)
    {
        var result = await _courseOfferingService.UpdateAsync(request);
        await _loggingHelper.LogOperationAsync("Updated", "CourseOffering", result.Id, $"ID: {result.Id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _courseOfferingService.DeleteAsync(id);
        await _loggingHelper.LogOperationAsync("Deleted", "CourseOffering", id, $"ID: {id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return NoContent();
    }   
}