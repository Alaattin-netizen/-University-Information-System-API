using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UIS.Application.Abstractions.StudentAbstractions;
using UIS.Application.DTOs.Student.Courses;
using UIS.Application.DTOs.Student.Messages;
using UIS.Application.DTOs.Student.Profile;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/students/me")]
[Authorize(Roles = "Student")]
public class StudentController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly IGradeService _gradeService;
    private readonly IScheduleService _scheduleService;
    private readonly IMessageService _messageService;
    private readonly IProfileService _profileService;

    public StudentController(
        IEnrollmentService enrollmentService,
        IGradeService gradeService,
        IScheduleService scheduleService,
        IMessageService messageService,
        IProfileService profileService)
    {
        _enrollmentService = enrollmentService;
        _gradeService = gradeService;
        _scheduleService = scheduleService;
        _messageService = messageService;
        _profileService = profileService;
    }

    private int GetStudentId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }

    // 1. List open courses
    [HttpGet("open-courses")]
    public async Task<IActionResult> GetOpenCourses()
    {
        var courses = await _enrollmentService.GetOpenCoursesAsync();
        return Ok(courses);
    }

    // 1. Enroll in a course
    [HttpPost("enroll")]
    public async Task<IActionResult> Enroll([FromBody] EnrollRequest request)
    {
        await _enrollmentService.EnrollAsync(GetStudentId(), request.CourseOfferingId);
        return Ok(new { message = "Successfully enrolled." });
    }

    // 2. Drop a course
    [HttpDelete("enrollments/{enrollmentId}")]
    public async Task<IActionResult> Drop(int enrollmentId)
    {
        await _enrollmentService.DropAsync(GetStudentId(), enrollmentId);
        return Ok(new { message = "Successfully dropped." });
    }

    // 3. View grades
    [HttpGet("grades")]
    public async Task<IActionResult> GetGrades()
    {
        var grades = await _gradeService.GetGradesAsync(GetStudentId());
        return Ok(grades);
    }

    // 3. View GPA
    [HttpGet("gpa")]
    public async Task<IActionResult> GetGPA()
    {
        var gpa = await _gradeService.GetGPAAsync(GetStudentId());
        return Ok(gpa);
    }

    // 4. View transcript
    [HttpGet("transcript")]
    public async Task<IActionResult> GetTranscript()
    {
        var transcript = await _gradeService.GetTranscriptAsync(GetStudentId());
        return Ok(transcript);
    }

    // 5. View weekly schedule
    [HttpGet("schedule")]
    public async Task<IActionResult> GetSchedule()
    {
        var schedule = await _scheduleService.GetWeeklyScheduleAsync(GetStudentId());
        return Ok(schedule);
    }

    // 6. Send message to advisor
    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        await _messageService.SendMessageAsync(GetStudentId(), request);
        return Ok(new { message = "Message sent to advisor." });
    }

    // 7. Update profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        await _profileService.UpdateProfileAsync(GetStudentId(), request);
        return Ok(new { message = "Profile updated successfully." });
    }
}