using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Application.Services;


namespace UIS.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : BaseApiController
{
    private readonly IFacultyService _facultyService;
    private readonly IUserService _userService;
    private readonly ISemesterService _semesterService;
    private readonly ILogService _logService;
    private readonly LoggingHelper _loggingHelper;

    public AdminController(
        IFacultyService facultyService,
        IUserService userService,
        ISemesterService semesterService,
        ILogService logService,
        LoggingHelper loggingHelper )
    {
        _facultyService = facultyService;
        _userService = userService;
        _semesterService = semesterService;
        _logService = logService;
        _loggingHelper = loggingHelper;
    }

    // ======================================================
    // FACULTY ENDPOINTS
    // ======================================================

    [HttpPost("faculties")]
    public async Task<IActionResult> CreateFaculty([FromBody] CreateFacultyRequest request)
    {
        var result = await _facultyService.CreateFacultyAsync(request);
        await _loggingHelper.LogOperationAsync(
           "Created",
           "Faculty",
           result.Id,
           $"Name: {request.Name}",
           GetCurrentUserId(),
           GetCurrentUserEmail(),
           GetCurrentUserRole()
       );

        return CreatedAtAction(nameof(CreateFaculty), new { id = result.Id }, result);


    }

    [HttpPost("departments")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
    {
        var result = await _facultyService.CreateDepartmentAsync(request);
        await _loggingHelper.LogOperationAsync(
         "Created",
         "Department",
         result.Id,
         $"Name: {request.Name}",
         GetCurrentUserId(),
         GetCurrentUserEmail(),
         GetCurrentUserRole()
     );
        return CreatedAtAction(nameof(CreateDepartment), new { id = result.Id }, result);
    }

    [HttpPost("courses")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var result = await _facultyService.CreateCourseAsync(request);
        await _loggingHelper.LogOperationAsync(
         "Created",
         "Course",
         result.Id,
         $"Name: {request.Name}",
         GetCurrentUserId(),
         GetCurrentUserEmail(),
         GetCurrentUserRole()
     );
        return CreatedAtAction(nameof(CreateCourse), new { id = result.Id }, result);
    }

    // ======================================================
    // USER ENDPOINTS (Create Students & Instructors)
    // ======================================================

    [HttpPost("users/student")]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
    {
        var result = await _userService.CreateStudentAsync(request);
        await _loggingHelper.LogOperationAsync(
       "Created",
       "Student",
       result.Id,
       $"Email: {request.Email}, Name: {request.FirstName} {request.LastName}",
       GetCurrentUserId(),
       GetCurrentUserEmail(),
       GetCurrentUserRole()
   );
        return CreatedAtAction(nameof(CreateStudent), new { id = result.Id }, result);
    }

    [HttpPost("users/instructor")]
    public async Task<IActionResult> CreateInstructor([FromBody] CreateInstructorRequest request)
    {
        var result = await _userService.CreateInstructorAsync(request);
        await _loggingHelper.LogOperationAsync(
         "Created",
         "Instructor",
         result.Id,
         $"Name: {request.FirstName} {request.LastName}",
         GetCurrentUserId(),
         GetCurrentUserEmail(),
         GetCurrentUserRole()
     );
        return CreatedAtAction(nameof(CreateInstructor), new { id = result.Id }, result);
    }

    // ======================================================
    // SEMESTER ENDPOINTS
    // ======================================================

    [HttpPost("semesters/open")]
    public async Task<IActionResult> OpenSemester([FromBody] CreateSemesterRequest request)
    {
        var semesterId = await _semesterService.OpenSemesterAsync(request);
        await _loggingHelper.LogOperationAsync(
         "Created",
         "Semester",
         semesterId,
         $"Name: {request.Name}",
         GetCurrentUserId(),
         GetCurrentUserEmail(),
         GetCurrentUserRole()
     );
        return Ok(new { message = "Semester opened successfully." });
    }

    [HttpPut("semesters/{semesterId}/calendar")]
    public async Task<IActionResult> UpdateRegistrationCalendar(int semesterId, [FromBody] UpdateSemesterRequest request)
    {
        await _semesterService.UpdateRegistrationCalenderAsync(semesterId, request);
        await _loggingHelper.LogOperationAsync(
         "Created",
         "Semester",
         semesterId,
         $"Name: null",
         GetCurrentUserId(),
         GetCurrentUserEmail(),
         GetCurrentUserRole()
     );
        return Ok(new { message = "Registration calendar updated successfully." });
    }

    // ======================================================
    // LOG ENDPOINTS
    // ======================================================

    [HttpGet("logs")]
    public async Task<IActionResult> GetAllLogs()
    {
        var logs = await _logService.GetAllLogsAsync(); // ✅ Now works
        return Ok(logs);
    }


    [HttpGet("logs/user/{userId}")]
    public async Task<IActionResult> GetUserLogs(int userId)
    {
        var logs = await _logService.GetLogsAsync(userId);
        return Ok(logs);
    }


}