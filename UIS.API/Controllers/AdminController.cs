using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin.AuditLog;
using UIS.Application.DTOs.Admin.Course;
using UIS.Application.DTOs.Admin.Department;
using UIS.Application.DTOs.Admin.Faculty;
using UIS.Application.DTOs.Admin.Semester;
using UIS.Application.DTOs.Admin.User;
using UIS.Application.DTOs.Admin.UserRole;
using UIS.Application.Services;
using UIS.Application.Services.AdminServices;

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
    private readonly IAuditLogService _auditLogService;
    private readonly IUserRoleService _userRoleService;
    private readonly LoggingHelper _loggingHelper;

    public AdminController(
        IFacultyService facultyService,
        IUserService userService,
        ISemesterService semesterService,
        IAuditLogService auditLogService,
        IUserRoleService userRoleService,
        ILogService logService,
        LoggingHelper loggingHelper)
    {
        _facultyService = facultyService;
        _userService = userService;
        _semesterService = semesterService;
        _auditLogService = auditLogService;
        _userRoleService = userRoleService;
        _logService = logService;
        _loggingHelper = loggingHelper;
    }

    // ======================================================
    // FACULTY CRUD
    // ======================================================

    [HttpGet("faculties")]
    public async Task<IActionResult> GetAllFaculties()
    {
        var result = await _facultyService.GetAllFacultiesAsync();
        return Ok(result);
    }

    [HttpGet("faculties/{id}")]
    public async Task<IActionResult> GetFacultyById(int id)
    {
        var result = await _facultyService.GetFacultyByIdAsync(id);
        return Ok(result);
    }

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
            GetCurrentUserRoles()
        );
        return CreatedAtAction(nameof(GetFacultyById), new { id = result.Id }, result);
    }

    [HttpPut("faculties")]
    public async Task<IActionResult> UpdateFaculty([FromBody] UpdateFacultyRequest request)
    {
        var result = await _facultyService.UpdateFacultyAsync(request);
        await _loggingHelper.LogOperationAsync(
            "Updated",
            "Faculty",
            result.Id,
            $"Name: {request.Name}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return Ok(result);
    }

    [HttpDelete("faculties/{id}")]
    public async Task<IActionResult> DeleteFaculty(int id)
    {
        await _facultyService.DeleteFacultyAsync(id);
        await _loggingHelper.LogOperationAsync(
            "Deleted",
            "Faculty",
            id,
            $"Faculty ID: {id}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return NoContent();
    }

    // ======================================================
    // DEPARTMENT CRUD
    // ======================================================

    [HttpGet("departments")]
    public async Task<IActionResult> GetAllDepartments()
    {
        var result = await _facultyService.GetAllDepartmentsAsync();
        return Ok(result);
    }

    [HttpGet("departments/{id}")]
    public async Task<IActionResult> GetDepartmentById(int id)
    {
        var result = await _facultyService.GetDepartmentByIdAsync(id);
        return Ok(result);
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
            GetCurrentUserRoles()
        );
        return CreatedAtAction(nameof(GetDepartmentById), new { id = result.Id }, result);
    }

    [HttpPut("departments")]
    public async Task<IActionResult> UpdateDepartment([FromBody] UpdateDepartmentRequest request)
    {
        var result = await _facultyService.UpdateDepartmentAsync(request);
        await _loggingHelper.LogOperationAsync(
            "Updated",
            "Department",
            result.Id,
            $"Name: {request.Name}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return Ok(result);
    }

    [HttpDelete("departments/{id}")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        await _facultyService.DeleteDepartmentAsync(id);
        await _loggingHelper.LogOperationAsync(
            "Deleted",
            "Department",
            id,
            $"Department ID: {id}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return NoContent();
    }

    // ======================================================
    // COURSE CRUD
    // ======================================================

    [HttpGet("courses")]
    public async Task<IActionResult> GetAllCourses()
    {
        var result = await _facultyService.GetAllCoursesAsync();
        return Ok(result);
    }

    [HttpGet("courses/{id}")]
    public async Task<IActionResult> GetCourseById(int id)
    {
        var result = await _facultyService.GetCourseByIdAsync(id);
        return Ok(result);
    }

    [HttpPost("courses")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var result = await _facultyService.CreateCourseAsync(request);
        await _loggingHelper.LogOperationAsync(
            "Created",
            "Course",
            result.Id,
            $"Code: {request.Code}, Name: {request.Name}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
    }

    [HttpPut("courses")]
    public async Task<IActionResult> UpdateCourse([FromBody] UpdateCourseRequest request)
    {
        var result = await _facultyService.UpdateCourseAsync(request);
        await _loggingHelper.LogOperationAsync(
            "Updated",
            "Course",
            result.Id,
            $"Code: {request.Code}, Name: {request.Name}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return Ok(result);
    }

    [HttpDelete("courses/{id}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        await _facultyService.DeleteCourseAsync(id);
        await _loggingHelper.LogOperationAsync(
            "Deleted",
            "Course",
            id,
            $"Course ID: {id}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return NoContent();
    }

    // ======================================================
    // SEMESTER CRUD
    // ======================================================

    [HttpGet("semesters")]
    public async Task<IActionResult> GetAllSemesters()
    {
        var result = await _semesterService.GetAllSemestersAsync();
        return Ok(result);
    }

    [HttpGet("semesters/{id}")]
    public async Task<IActionResult> GetSemesterById(int id)
    {
        var result = await _semesterService.GetSemesterByIdAsync(id);
        return Ok(result);
    }

    [HttpPost("semesters")]
    public async Task<IActionResult> CreateSemester([FromBody] CreateSemesterRequest request)
    {
        var result = await _semesterService.CreateSemesterAsync(request);
        await _loggingHelper.LogOperationAsync(
            "Created",
            "Semester",
            result.Id,
            $"Name: {request.Name}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return CreatedAtAction(nameof(GetSemesterById), new { id = result.Id }, result);
    }

    [HttpPut("semesters")]
    public async Task<IActionResult> UpdateSemester([FromBody] UpdateSemesterRequest request)
    {
        var result = await _semesterService.UpdateSemesterAsync(request);
        await _loggingHelper.LogOperationAsync(
            "Updated",
            "Semester",
            result.Id,
            $"Name: {request.Name}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return Ok(result);
    }

    [HttpDelete("semesters/{id}")]
    public async Task<IActionResult> DeleteSemester(int id)
    {
        await _semesterService.DeleteSemesterAsync(id);
        await _loggingHelper.LogOperationAsync(
            "Deleted",
            "Semester",
            id,
            $"Semester ID: {id}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return NoContent();
    }

    [HttpPut("semesters/{semesterId}/calendar")]
    public async Task<IActionResult> UpdateRegistrationCalendar(int semesterId, [FromBody] UpdateRegistrationDateRequest request)
    {
        var result = await _semesterService.UpdateRegistrationCalendarAsync(semesterId, request);
        await _loggingHelper.LogOperationAsync(
            "Updated",
            "Semester",
            semesterId,
            $"Updated registration calendar for Semester ID: {semesterId}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return Ok(result);
    }

    // ======================================================
    // USER CRUD (Students, Instructors, Admins)
    // ======================================================

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _userService.GetAllUsersAsync();
        return Ok(result);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return Ok(result);
    }

    [HttpPost("users/student")]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
    {
        var result = await _userService.CreateStudentAsync(request);
        await _loggingHelper.LogOperationAsync(
            "Created",
            "Student",
            result.Id,
            $"Email: {request.Email}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
    }

    [HttpPost("users/instructor")]
    public async Task<IActionResult> CreateInstructor([FromBody] CreateInstructorRequest request)
    {
        var result = await _userService.CreateInstructorAsync(request);
        await _loggingHelper.LogOperationAsync(
            "Created",
            "Instructor",
            result.Id,
            $"Email: {request.Email}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
    }

    [HttpPost("users/admin")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest request)
    {
        var result = await _userService.CreateAdminAsync(request);
        await _loggingHelper.LogOperationAsync(
            "Created",
            "Admin",
            result.Id,
            $"Email: {request.Email}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
    }

    [HttpPost("users/assign-admin")]
    public async Task<IActionResult> AssignAdminRole([FromBody] AssignAdminRoleRequest request)
    {
        var result = await _userService.AssignAdminRoleAsync(request);
        await _loggingHelper.LogOperationAsync(
            "AssignedRole",
            "User",
            result.Id,
            $"User {result.Email} was granted Admin role",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return Ok(result);
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteUserAsync(id);
        await _loggingHelper.LogOperationAsync(
            "Deleted",
            "User",
            id,
            $"User ID: {id}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );
        return NoContent();
    }

    // ======================================================
    // AUDIT LOGS
    // ======================================================

    [HttpPost("audit-logs")]
    public async Task<IActionResult> CreateAuditLog([FromBody] CreateAuditLogRequest request)
    {
        var result = await _auditLogService.CreateAsync(request);
        await _loggingHelper.LogOperationAsync("Created", "AuditLog", result.Id, $"Action: {request.Action}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return Ok(result);
    }

    [HttpPut("audit-logs")]
    public async Task<IActionResult> UpdateAuditLog([FromBody] UpdateAuditLogRequest request)
    {
        var result = await _auditLogService.UpdateAsync(request);
        await _loggingHelper.LogOperationAsync("Updated", "AuditLog", result.Id, $"ID: {result.Id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return Ok(result);
    }

    [HttpDelete("audit-logs/{id}")]
    public async Task<IActionResult> DeleteAuditLog(int id)
    {
        await _auditLogService.DeleteAsync(id);
        await _loggingHelper.LogOperationAsync("Deleted", "AuditLog", id, $"ID: {id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return NoContent();
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetAllLogs()
    {
        var result = await _logService.GetAllLogsAsync();
        return Ok(result);
    }



    [HttpGet("logs/user/{userId}")]
    public async Task<IActionResult> GetUserLogs(int userId)
    {
        var result = await _logService.GetLogsAsync(userId);
        return Ok(result);
    }

    // ======================================================
    // USER ROLES
    // ======================================================
    [HttpGet("user-roles")]
    public async Task<IActionResult> GetAllUserRoles()
    {
        return Ok(await _userRoleService.GetAllAsync());
    }

    [HttpGet("user-roles/{id}")]
    public async Task<IActionResult> GetUserRoleById(int id)
    {
        return Ok(await _userRoleService.GetByIdAsync(id));
    }

    [HttpGet("user-roles/user/{userId}")]
    public async Task<IActionResult> GetUserRolesByUser(int userId)
    {
        return Ok(await _userRoleService.GetByUserIdAsync(userId));
    }

    [HttpGet("user-roles/role/{roleId}")]
    public async Task<IActionResult> GetUserRolesByRole(int roleId)
    {
        return Ok(await _userRoleService.GetByRoleIdAsync(roleId));
    }

    [HttpPost("user-roles/assign")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
    {
        var result = await _userRoleService.AssignRoleAsync(request);
        await _loggingHelper.LogOperationAsync("Assigned", "UserRole", result.Id, $"User: {request.UserId}, Role: {request.RoleId}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return CreatedAtAction(nameof(GetUserRoleById), new { id = result.Id }, result);
    }

    [HttpDelete("user-roles/remove")]
    public async Task<IActionResult> RemoveRole([FromBody] RemoveRoleRequest request)
    {
        await _userRoleService.RemoveRoleAsync(request);
        await _loggingHelper.LogOperationAsync("Removed", "UserRole", null, $"User: {request.UserId}, Role: {request.RoleId}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return NoContent();
    }
}