using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UIS.Application.Abstractions.InstructorAbstractions;
using UIS.Application.DTOs.Instructor;
using UIS.Application.DTOs.Student.Courses;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/instructors/me")]
[Authorize(Roles = "Instructor")]
public class InstructorController : ControllerBase
{
    private readonly ICourseService _CourseService;
    private readonly IStudentService _StudentService;
    

    public InstructorController(
        ICourseService courseService,
        IStudentService studentService)
    {
        _CourseService = courseService;
        _StudentService = studentService;
    }

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
        var students = await _CourseService.GetRegisteredStudentsAsync(GetInstructorId(), courseOfferingId);
        return Ok(students);
    }

    //3. Create announcement
    [HttpPost("Announcements")]
    public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementRequest request)
    {
        await _CourseService.CreateAnnouncementAsync(GetInstructorId(), request);
        return Ok(new { message = "Announcement created successfully." });
    }

    //4. Enter grades
    [HttpPost("Enter-Grades")]
    public async Task<IActionResult> EnterGrades([FromBody] GradeEntryRequest request)
    {
        await _StudentService.EnterGradesAsync(GetInstructorId(), request);
        return Ok(new { message = "Grades entered successfully." });
    }

    //5. Enter attendance
    [HttpPost("Enter-Attendance")]
    public async Task<IActionResult> EnterAttendance([FromBody] AttendanceEntryRequest request)
    {
        await _StudentService.EnterAttendanceAsync(GetInstructorId(), request);
        return Ok(new { message = "Attendance entered successfully." });
    }




}