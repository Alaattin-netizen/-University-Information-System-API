using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UIS.Application.Abstractions.InstructorAbstractions;
using UIS.Application.Abstractions.StudentAbstractions;
using UIS.Application.DTOs.Instructor;
using UIS.Application.Services;
namespace UIS.API.Controllers;

[ApiController]
[Route("api/instructors/me")]
[Authorize(Roles = "Instructor")]
public class InstructorController : BaseApiController
{
    private readonly ICourseService _CourseService;
    private readonly IStudentService _StudentService;
    private readonly LoggingHelper _loggingHelper;
    private readonly IMessageService _messageService;

    public InstructorController(
        ICourseService courseService,
        IStudentService studentService,
        LoggingHelper loggingHelper,
        IMessageService messageService)
    {
        _CourseService = courseService;
        _StudentService = studentService;
        _loggingHelper = loggingHelper;
        _messageService = messageService;
    }

    [HttpGet("messages")]
    public async Task<IActionResult> GetMessages()
        => Ok(await _messageService.GetReceivedMessagesAsync(GetInstructorId()));

    private int GetInstructorId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }

    // 1. List open courses
    [HttpGet("Responsible-Courses")]
    public async Task<IActionResult> GetMyCourses()
    {
        var courses = await _CourseService.GetMyCoursesAsync(GetInstructorId());
        return Ok(courses);
    }

    // 2. List registered students for a course,
    [HttpGet("Responsible-Courses/{courseOfferingId}/Registered-Students")]
    public async Task<IActionResult> GetRegisteredStudents(int courseOfferingId)
    {
        try
        {
            var students = await _CourseService.GetRegisteredStudentsAsync(GetInstructorId(), courseOfferingId);
            return Ok(students);
        }

        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                                   || ex.Message.Contains("permission", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("Responsible-Courses/by-date")]
    public async Task<IActionResult> GetMyCoursesForDate([FromQuery] DateTime date)
    {
        return Ok(await _CourseService.GetMyCoursesForDateAsync(GetInstructorId(), date));
    }

    [HttpGet("Responsible-Courses/{courseOfferingId}/Announcements")]
    public async Task<IActionResult> GetAnnouncements(int courseOfferingId)
    {
        return Ok(await _CourseService.GetAnnouncementsAsync(GetInstructorId(), courseOfferingId));
    }

    //3. Create announcement
    [HttpPost("Announcements")]
    public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementRequest request)
    {
        await _CourseService.CreateAnnouncementAsync(GetInstructorId(), request);
        await _loggingHelper.LogOperationAsync(
        "Created",
        "Announcement",
        null,
        $"Title: {request.Title}, CourseOfferingId: {request.CourseOfferingId}",
        GetCurrentUserId(),
        GetCurrentUserEmail(),
        GetCurrentUserRoles()
    );
        return Ok(new { message = "Announcement created successfully." });
    }

    //4. Enter grades
    [HttpPost("Enter-Grades")]
    public async Task<IActionResult> EnterGrades([FromBody] GradeEntryRequest request)
    {
        try
        {
            await _StudentService.EnterGradesAsync(GetInstructorId(), request);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                                   || ex.Message.Contains("permission", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = ex.Message });
        }
        await _loggingHelper.LogOperationAsync(
     "Updated",
     "Grade",
     request.EnrollmentId,
     $"Instructor: {GetCurrentUserEmail()}, EnrollmentId: {request.EnrollmentId}",
     GetCurrentUserId(),
     GetCurrentUserEmail(),
     GetCurrentUserRoles()
 );
        return Ok(new { message = "Grades entered successfully." });
    }

    //5. Enter attendance
    [HttpPost("Enter-Attendance")]
    public async Task<IActionResult> EnterAttendance([FromBody] AttendanceEntryRequest request)
    {
        try
        {
            await _StudentService.EnterAttendanceAsync(GetInstructorId(), request);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        await _loggingHelper.LogOperationAsync(
     "Updated",
     "Attendance",
     request.StudentId,
     $"Date: {request.Date:yyyy-MM-dd}, Present: {request.IsPresent}",
     GetCurrentUserId(),
     GetCurrentUserEmail(),
     GetCurrentUserRoles()
 );
        return Ok(new { message = "Attendance entered successfully." });
    }




}