using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Infrastructure.Repositories;
using System.Security.Claims;
using UIS.Application.Abstractions.StudentAbstractions;
using UIS.Application.DTOs.Student.Courses;
using UIS.Application.DTOs.Student.Messages;
using UIS.Application.Services;
using UIS.Domain.Entities;
using UIS.Application.DTOs.Profile;
namespace UIS.API.Controllers;

[ApiController]
[Route("api/students/me")]
[Authorize(Roles = "Student")]
public class StudentController : BaseApiController
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly IGradeService _gradeService;
    private readonly IScheduleService _scheduleService;
    private readonly IMessageService _messageService;
    private readonly LoggingHelper _loggingHelper;

    private readonly IUnitOfWork _unitOfWork;

    public StudentController(
        IUnitOfWork unitOfWork,
        IEnrollmentService enrollmentService,
        IGradeService gradeService,
        IScheduleService scheduleService,
        IMessageService messageService,
        LoggingHelper loggingHelper)
    {
        _enrollmentService = enrollmentService;
        _gradeService = gradeService;
        _scheduleService = scheduleService;
        _messageService = messageService;
        _loggingHelper = loggingHelper;
        _unitOfWork = unitOfWork;
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
        // Call the service (it now saves the enrollment)
        await _enrollmentService.EnrollAsync(GetStudentId(), request.CourseOfferingId);

        // Since we don't return the ID from the service, fetch it
        var enrollment = await _unitOfWork.Repository<Enrollment>()
            .GetFirstAsync(e => e.StudentId == GetStudentId() && e.CourseOfferingId == request.CourseOfferingId && e.IsActive);

        await _loggingHelper.LogOperationAsync(
            "Created",
            "Enrollment",
            enrollment?.Id,
            $"StudentId: {GetStudentId()}, CourseOfferingId: {request.CourseOfferingId}",
            GetCurrentUserId(),
            GetCurrentUserEmail(),
            GetCurrentUserRoles()
        );

        return Ok(new { message = "Successfully enrolled." });
    }

    // 2. Drop a course
    [HttpDelete("enrollments/{enrollmentId}")]
    public async Task<IActionResult> Drop(int enrollmentId)
    {
        await _enrollmentService.DropAsync(GetStudentId(), enrollmentId);
        await _loggingHelper.LogOperationAsync(
      "Deleted",
      "Enrollment",
      enrollmentId,
      $"StudentId: {GetStudentId()}, EnrollmentId: {enrollmentId}",
      GetCurrentUserId(),
      GetCurrentUserEmail(),
      GetCurrentUserRoles()
  );
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
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage( [FromBody] SendMessageRequest request)
    {
        await _messageService.SendMessageAsync(GetStudentId(), request);
        await _loggingHelper.LogOperationAsync(
       "Created",
       "Message",
       null, // ID not returned
       $"Subject: {request.Subject}, To: Advisor",
       GetCurrentUserId(),
       GetCurrentUserEmail(),
       GetCurrentUserRoles()
   );
        return Ok(new { message = "Message sent to advisor." });
    }


    [HttpGet("enrollments")]
    public async Task<IActionResult> GetMyEnrollments()
    {
        var studentId = GetStudentId();
        var enrollments = await _enrollmentService.GetActiveEnrollmentsAsync(studentId);
        return Ok(enrollments);
    }

    [HttpGet("messages")]
    public async Task<IActionResult> GetMyMessages()
    {
        var studentId = GetStudentId();
        var messages = await _messageService.GetSentMessagesAsync(studentId);
        return Ok(messages);
    }
}