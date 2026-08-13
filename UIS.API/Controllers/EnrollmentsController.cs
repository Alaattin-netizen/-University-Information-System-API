using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Application.Services;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/enrollments")]
[Authorize(Roles = "Admin")]
public class EnrollmentsController : BaseApiController
{
    private readonly IAdminEnrollmentService _enrollmentService;
    private readonly LoggingHelper _loggingHelper;

    public EnrollmentsController(IAdminEnrollmentService enrollmentService, LoggingHelper loggingHelper)
    {
        _enrollmentService = enrollmentService;
        _loggingHelper = loggingHelper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _enrollmentService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _enrollmentService.GetByIdAsync(id));

    [HttpGet("student/{studentId}")]
    public async Task<IActionResult> GetByStudent(int studentId)
        => Ok(await _enrollmentService.GetByStudentAsync(studentId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        var result = await _enrollmentService.CreateAsync(request);
        await _loggingHelper.LogOperationAsync("Created", "Enrollment", result.Id, $"Student: {result.StudentId}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateEnrollmentRequest request)
    {
        var result = await _enrollmentService.UpdateAsync(request);
        await _loggingHelper.LogOperationAsync("Updated", "Enrollment", result.Id, $"ID: {result.Id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _enrollmentService.DeleteAsync(id);
        await _loggingHelper.LogOperationAsync("Deleted", "Enrollment", id, $"ID: {id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return NoContent();
    }
}