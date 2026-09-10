using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.InstructorAbstractions;
using UIS.Application.DTOs.Instructor;
using UIS.Application.DTOs.Admin;
using UIS.Infrastructure.Repositories;
using UIS.Domain.Entities;
namespace UIS.Application.Services.InstructorServices;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;

    public CourseService(IUnitOfWork unitOfWork)

    {
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

    public async Task<IEnumerable<CourseResponse>> GetMyCoursesForDateAsync(int instructorId, DateTime date)
    {
        var courses = await GetMyCoursesAsync(instructorId);
        return courses.Where(course =>
            Enum.TryParse<DayOfWeek>(course.Day, true, out var day)
            && day == date.DayOfWeek);
    }

    public async Task<IEnumerable<RegisteredStudentResponse>> GetRegisteredStudentsAsync(int instructorId, int courseOfferingId, DateTime? date = null)
    {
        // Verify the instructor owns this course offering
        var offering = await _unitOfWork.Repository<CourseOffering>()
            .GetQueryable()
            .Include(o => o.Enrollments)
                .ThenInclude(e => e.Student)
                .Include(o => o.Semester)
                .FirstOrDefaultAsync(o => o.Id == courseOfferingId
                                          && o.InstructorId == instructorId
                                          && o.Semester.IsActive);

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
            var attendanceForDate = date.HasValue
                ? await _unitOfWork.Repository<Attendance>()
                    .GetQueryable()
                    .Where(a => a.StudentId == enrollment.StudentId
                        && a.CourseOfferingId == courseOfferingId
                        && a.Date.Date == date.Value.Date)
                    .Select(a => (bool?)a.IsPresent)
                    .FirstOrDefaultAsync()
                : null;

            result.Add(new RegisteredStudentResponse
            {
                StudentId = enrollment.StudentId,
                FullName = $"{enrollment.Student.FirstName} {enrollment.Student.LastName}",
                Email = enrollment.Student.Email,
                MidtermScore = enrollment.MidtermScore,
                EnrollmentId = enrollment.Id, 
                FinalScore = enrollment.FinalScore,
                AssignmentScore = enrollment.AssignmentScore,
                MakeupScore = enrollment.MakeupScore,
                TotalScore = enrollment.TotalScore,
                LetterGrade = enrollment.LetterGrade,
                GradePoint = enrollment.GradePoint,
                AttendanceCount = attendanceCount,
                TotalClasses = Math.Max(totalClasses, 1),
                IsPresent = attendanceForDate
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
    }

    public async Task<IEnumerable<AnnouncementResponse>> GetAnnouncementsAsync(int instructorId, int courseOfferingId)
    {
        var announcements = await _unitOfWork.Repository<Announcement>()
            .GetQueryable()
            .Include(a => a.CourseOffering)
                .ThenInclude(o => o.Course)
            .Where(a => a.InstructorId == instructorId && a.CourseOfferingId == courseOfferingId)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();

        return announcements.Select(a => new AnnouncementResponse
        {
            Id = a.Id,
            Title = a.Title,
            Content = a.Content,
            CreatedDate = a.CreatedDate,
            CourseOfferingId = a.CourseOfferingId,
            InstructorId = a.InstructorId,
            CourseCode = a.CourseOffering.Course.Code,
        });
    }

   
}