using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IFacultyService _facultyService;
    private readonly IUserService _userService;
    private readonly ISemesterService _semesterService;
    private readonly ILogService _logService;

    public AdminController(
        IFacultyService facultyService,
        IUserService userService,
        ISemesterService semesterService,
        ILogService logService)
    {
        _facultyService = facultyService;
        _userService = userService;
        _semesterService = semesterService;
        _logService = logService;
    }

    // ======================================================
    // FACULTY ENDPOINTS
    // ======================================================

    [HttpPost("faculties")]
    public async Task<IActionResult> CreateFaculty([FromBody] CreateFacultyRequest request)
    {
        var result = await _facultyService.CreateFacultyAsync(request);
        return CreatedAtAction(nameof(CreateFaculty), new { id = result.Id }, result);
    }

    [HttpPost("departments")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
    {
        var result = await _facultyService.CreateDepartmentAsync(request);
        return CreatedAtAction(nameof(CreateDepartment), new { id = result.Id }, result);
    }

    [HttpPost("courses")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var result = await _facultyService.CreateCourseAsync(request);
        return CreatedAtAction(nameof(CreateCourse), new { id = result.Id }, result);
    }

    // ======================================================
    // USER ENDPOINTS (Create Students & Instructors)
    // ======================================================

    [HttpPost("users/student")]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
    {
        var result = await _userService.CreateStudentAsync(request);
        return CreatedAtAction(nameof(CreateStudent), new { id = result.Id }, result);
    }

    [HttpPost("users/instructor")]
    public async Task<IActionResult> CreateInstructor([FromBody] CreateInstructorRequest request)
    {
        var result = await _userService.CreateInstructorAsync(request);
        return CreatedAtAction(nameof(CreateInstructor), new { id = result.Id }, result);
    }

    // ======================================================
    // SEMESTER ENDPOINTS
    // ======================================================

    [HttpPost("semesters/open")]
    public async Task<IActionResult> OpenSemester([FromBody] CreateSemesterRequest request)
    {
        await _semesterService.OpenSemesterAsync(request);
        return Ok(new { message = "Semester opened successfully." });
    }

    [HttpPut("semesters/{semesterId}/calendar")]
    public async Task<IActionResult> UpdateRegistrationCalendar(int semesterId, [FromBody] UpdateSemesterRequest request)
    {
        await _semesterService.UpdateRegistrationCalenderAsync(semesterId, request);
        return Ok(new { message = "Registration calendar updated successfully." });
    }

    // ======================================================
    // LOG ENDPOINTS
    // ======================================================

    [HttpGet("logs")]
    public async Task<IActionResult> GetAllLogs()
    {
        // If your ILogService has a method to get all logs, use it.
        // Otherwise, we can get logs for a specific user or all users.
        var logs = await _logService.GetLogsAsync(0); // 0 = all users (if implemented)
        return Ok(logs);
    }

    [HttpGet("logs/user/{userId}")]
    public async Task<IActionResult> GetUserLogs(int userId)
    {
        var logs = await _logService.GetLogsAsync(userId);
        return Ok(logs);
    }
}