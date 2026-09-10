using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Application.DTOs.Filters;
using UIS.Application.Services;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/attendances")]
public class AttendancesController : BaseApiController
{
    private readonly IAttendanceService _attendanceService;
    private readonly LoggingHelper _loggingHelper;

    public AttendancesController(IAttendanceService attendanceService, LoggingHelper loggingHelper)
    {
        _attendanceService = attendanceService;
        _loggingHelper = loggingHelper;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] AttendanceFilterRequest request)
        => Ok(await _attendanceService.GetAllAsync(request));

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _attendanceService.GetByIdAsync(id));

    [HttpGet("student/{studentId}")]
    [Authorize]
    public async Task<IActionResult> GetByStudent(int studentId)
    {
        // Only allow if admin, or if the user is the student themselves (or instructor of their courses?)
        var currentUserId = GetCurrentUserId();
        var roles = GetCurrentUserRoles();
        if (roles.Contains("Admin") || currentUserId == studentId)
        {
            return Ok(await _attendanceService.GetByStudentAsync(studentId));
        }
        // For instructors: they can see attendance for students in their courses, but we'd need to check.
        // For simplicity, we keep this as Admin-only or self.
        return Forbid();
    }

    [HttpGet("course-offering/{courseOfferingId}")]
    [Authorize]
    public async Task<IActionResult> GetByCourseOffering(int courseOfferingId)
    {
        // Admin or instructor of that course
        // For now, let Admin only, or instructor (need to check)
        var roles = GetCurrentUserRoles();
        if (roles.Contains("Admin"))
            return Ok(await _attendanceService.GetByCourseOfferingAsync(courseOfferingId));
        else
            // We could check if current user teaches this course
            return Forbid();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateAttendanceRequest request)
    {
        var result = await _attendanceService.CreateAsync(request);
        await _loggingHelper.LogOperationAsync("Created", "Attendance", result.Id, $"Student: {result.StudentId}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateAttendanceRequest request)
    {
        var result = await _attendanceService.UpdateAsync(request);
        await _loggingHelper.LogOperationAsync("Updated", "Attendance", result.Id, $"ID: {result.Id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _attendanceService.DeleteAsync(id);
        await _loggingHelper.LogOperationAsync("Deleted", "Attendance", id, $"ID: {id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return NoContent();
    }

    [HttpGet("export")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export([FromQuery] AttendanceFilterRequest request)
    {
        var file = await _attendanceService.ExportAsync(request);
        return File(file,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"attendance-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
    }

    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "An Excel file is required." });
        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only .xlsx files are supported." });

        try
        {
            await using var stream = file.OpenReadStream();
            return Ok(await _attendanceService.ImportAsync(stream));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex) when (ex is FormatException || ex.GetType().Namespace?.StartsWith("ClosedXML", StringComparison.Ordinal) == true)
        {
            return BadRequest(new { message = $"The attendance file could not be read: {ex.Message}" });
        }
    }
}