using Hangfire;
using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions;
using UIS.Application.Abstractions.InstructorAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Application.DTOs.Instructor;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;
namespace UIS.Application.Services.InstructorServices;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IBackgroundJobService _backgroundJobs;
    public CourseService(IUnitOfWork unitOfWork, ICacheService cache, IBackgroundJobService backgroundJobs)

    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _backgroundJobs = backgroundJobs;
    }

    public async Task<IEnumerable<CourseResponse>> GetMyCoursesAsync(int instructorId)
    {
        var cacheKey = $"instructor-courses:{instructorId}";

        var cachedCourses =
            await _cache.GetAsync<List<CourseResponse>>(cacheKey);

        if (cachedCourses is not null)
        {
            Console.WriteLine(
                $"REDIS HIT: {cacheKey} - {cachedCourses.Count} courses"
            );

            return cachedCourses;
        }

        Console.WriteLine($"REDIS MISS: {cacheKey}");

        var offerings = await _unitOfWork.Repository<CourseOffering>()
            .GetQueryable()
            .Include(o => o.Course)
            .Include(o => o.Enrollments)
            .Include(o => o.Semester)
            .Where(o => o.InstructorId == instructorId && o.Semester.IsActive)
            .ToListAsync();

        var result = offerings.Select(o => new CourseResponse
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
        }).ToList();

        await _cache.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(10)
        );

        Console.WriteLine(
            $"REDIS SET: {cacheKey} - {result.Count} courses"
        );

        return result;
    }

    public async Task<IEnumerable<CourseResponse>> GetMyCoursesForDateAsync(int instructorId, DateTime date)
    {
        var cacheKey = $"instructor-courses:{instructorId}:{date:yyyy-MM-dd}";

        var cachedCourses =
            await _cache.GetAsync<List<CourseResponse>>(cacheKey);

        if (cachedCourses is not null)
        {
            Console.WriteLine(
                $"REDIS HIT: {cacheKey} - {cachedCourses.Count} courses"
            );

            return cachedCourses;
        }

        Console.WriteLine($"REDIS MISS: {cacheKey}");

        var courses = await GetMyCoursesAsync(instructorId);
        var result = courses.Where(course =>
            Enum.TryParse<DayOfWeek>(course.Day, true, out var day)
            && day == date.DayOfWeek).ToList();

        await _cache.SetAsync(
          cacheKey,
          result,
          TimeSpan.FromMinutes(10)
      );

        Console.WriteLine(
            $"REDIS SET: {cacheKey} - {result.Count} courses"
        );
        return result;
    }

    public async Task<IEnumerable<RegisteredStudentResponse>> GetRegisteredStudentsAsync(int instructorId, int courseOfferingId, DateTime? date = null)
    {
        Console.WriteLine(
       $"===== GetRegisteredStudentsAsync CALLED: instructor={instructorId}, offering={courseOfferingId} ====="
   );
        var cacheKey =
            $"registered-students:{instructorId}:{courseOfferingId}:{date?.Date:yyyy-MM-dd}";
        var cachedStudents =
       await _cache.GetAsync<List<RegisteredStudentResponse>>(cacheKey);

        if (cachedStudents is not null)
        {
            Console.WriteLine(
    $"REDIS HIT: {cacheKey} - {cachedStudents.Count} students"
);

            return cachedStudents;
        }

        Console.WriteLine($"REDIS MISS: {cacheKey}");

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
            var attendanceCount = await _unitOfWork.Repository<Attendance>()
                .GetQueryable()
                .Where(a => a.StudentId == enrollment.StudentId && a.CourseOfferingId == courseOfferingId)
                .CountAsync(a => a.IsPresent);

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
        await _cache.SetAsync(
    cacheKey,
    result,
    TimeSpan.FromMinutes(10)
);


        return result;
    }

    public async Task CreateAnnouncementAsync(int instructorId, CreateAnnouncementRequest request)
    {
        var offering = await _unitOfWork.Repository<CourseOffering>()
            .GetQueryable()
            .Include(o => o.Course)
            .Include(o => o.Enrollments)
                .ThenInclude(e => e.Student)  
            .FirstOrDefaultAsync(o => o.Id == request.CourseOfferingId
                                    && o.InstructorId == instructorId);

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

        await _cache.RemoveAsync($"announcements:{instructorId}:{request.CourseOfferingId}");

        var courseCode = offering.Course.Code;
        var courseName = offering.Course.Name;
        var title = request.Title;
        var content = request.Content;

        foreach (var enrollment in offering.Enrollments.Where(e => e.IsActive))
        {
            var studentEmail = enrollment.Student.Email;
            var studentFirstName = enrollment.Student.FirstName;

            _backgroundJobs.Enqueue<IEmailService>(x => x.SendEmailAsync(
                studentEmail,
                $"New Announcement: {courseCode} - {title}",
                $"Dear {studentFirstName},\n\n" +
                $"A new announcement has been posted for {courseCode} - {courseName}:\n\n" +
                $"**{title}**\n\n" +
                $"{content}\n\n" +
                $"Best regards,\nUniversity Information System",
                false
            ));
        }

    }

    public async Task<IEnumerable<AnnouncementResponse>> GetAnnouncementsAsync(int instructorId, int courseOfferingId)
    {
        var cacheKey =
       $"announcements:{instructorId}:{courseOfferingId}";
        var cachedAnnouncements =
       await _cache.GetAsync<List<AnnouncementResponse>>(cacheKey);

        if (cachedAnnouncements is not null)
        {
            Console.WriteLine(
    $"REDIS HIT: {cacheKey} - {cachedAnnouncements.Count} announcements"
);

            return cachedAnnouncements;
        }

        Console.WriteLine($"REDIS MISS: {cacheKey}");
        var announcements = await _unitOfWork.Repository<Announcement>()
            .GetQueryable()
            .Include(a => a.CourseOffering)
                .ThenInclude(o => o.Course)
            .Where(a => a.InstructorId == instructorId && a.CourseOfferingId == courseOfferingId)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();

        var result = announcements.Select(a => new AnnouncementResponse
        {
            Id = a.Id,
            Title = a.Title,
            Content = a.Content,
            CreatedDate = a.CreatedDate,
            CourseOfferingId = a.CourseOfferingId,
            InstructorId = a.InstructorId,
            CourseCode = a.CourseOffering.Course.Code,
        }).ToList();

        await _cache.SetAsync(
  cacheKey,
  result,
  TimeSpan.FromMinutes(10)
);

        return result;

    }

   
}