using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.InstructorAbstractions;
using UIS.Application.DTOs.Instructor;
using UIS.Infrastructure.Repositories;
using UIS.Domain.Entities;
namespace UIS.Application.Services.InstructorServices;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly LoggingHelper _loggingHelper;  

    public CourseService(IUnitOfWork unitOfWork, LoggingHelper loggingHelper)

    {
        _loggingHelper = loggingHelper;
        _unitOfWork = unitOfWork;
    }

    // 1. List courses the instructor is responsible for
    public async Task<IEnumerable<CourseResponse>> GetMyCoursesAsync(int instructorId)
    {
        var offerings = await _unitOfWork.Repository<CourseOffering>()
            .GetQueryable()
            .Include(o => o.Course)
            .Include(o => o.Enrollments)
            .Include(o => o.Semester)
            .Where(o => o.InstructorId == instructorId && o.Semester.IsActive)
            .ToListAsync();

        return offerings.Select(o => new CourseResponse
        {
            CourseOfferingId = o.Id,
            CourseCode = o.Course.Code,
            CourseName = o.Course.Name,
            Credits = o.Course.Credits,
            Day = o.Day.ToString(),
            StartTime = o.StartTime.ToString(@"hh\:mm"),
            EndTime = o.EndTime.ToString(@"hh\:mm"),
            Classroom = o.Classroom,
            EnrolledStudentsCount = o.Enrollments.Count(e => e.IsActive),
            Quota = o.Course.Quota
        });
    }

    public async Task<IEnumerable<RegisteredStudentResponse>> GetRegisteredStudentsAsync(int instructorId, int courseOfferingId)
    {
        // Verify the instructor owns this course offering
        var offering = await _unitOfWork.Repository<CourseOffering>()
            .GetQueryable()
            .Include(o => o.Enrollments)
                .ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(o => o.Id == courseOfferingId && o.InstructorId == instructorId);

        if (offering == null)
            throw new Exception("Course offering not found or you don't have permission.");

        var enrollments = offering.Enrollments.Where(e => e.IsActive).ToList();
        var result = new List<RegisteredStudentResponse>();

        foreach (var enrollment in enrollments)
        {
            // Get attendance count for this student in this course
            var attendanceCount = await _unitOfWork.Repository<Attendance>()
                .GetQueryable()
                .Where(a => a.StudentId == enrollment.StudentId && a.CourseOfferingId == courseOfferingId)
                .CountAsync(a => a.IsPresent);

            // Get total classes scheduled so far
            var totalClasses = await _unitOfWork.Repository<Attendance>()
                .GetQueryable()
                .Where(a => a.CourseOfferingId == courseOfferingId)
                .Select(a => a.Date)
                .Distinct()
                .CountAsync();

            result.Add(new RegisteredStudentResponse
            {
                StudentId = enrollment.StudentId,
                FullName = $"{enrollment.Student.FirstName} {enrollment.Student.LastName}",
                Email = enrollment.Student.Email,
                MidtermScore = enrollment.MidtermScore,
                FinalScore = enrollment.FinalScore,
                TotalScore = enrollment.TotalScore,
                LetterGrade = enrollment.LetterGrade,
                GradePoint = enrollment.GradePoint,
                AttendanceCount = attendanceCount,
                TotalClasses = Math.Max(totalClasses, 1)
            });
        }

        return result;
    }

    public async Task CreateAnnouncementAsync(int instructorId, CreateAnnouncementRequest request)
    {
        // Verify the instructor owns this course offering
        var offering = await _unitOfWork.Repository<CourseOffering>().GetQueryable()
            .FirstOrDefaultAsync(o => o.Id == request.CourseOfferingId && o.InstructorId == instructorId);

        if (offering == null)
            throw new Exception("Course offering not found or you don't have permission.");

        var announcement = new Announcement
        {
            Title = request.Title,
            Content = request.Content,
            CreatedDate = DateTime.UtcNow,
            CourseOfferingId = request.CourseOfferingId,
            InstructorId = instructorId
        };

        await _unitOfWork.Repository<Announcement>().AddAsync(announcement);
        await _unitOfWork.SaveChangesAsync();

        await _loggingHelper.LogOperationAsync(
     "Created",
     "Announcement",
     announcement.Id,
     $"Title: {request.Title}, CourseOfferingId: {request.CourseOfferingId}"
 );

    }

   
}