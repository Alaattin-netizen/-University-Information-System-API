using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions;
using UIS.Application.Abstractions.StudentAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Application.DTOs.Student.Courses;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;
namespace UIS.Application.Services.StudentServices;

using Hangfire;

public class EnrollmentService : IEnrollmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IBackgroundJobService _backgroundJobs; // ✅ Injected



    public EnrollmentService(IUnitOfWork unitOfWork, ICacheService cache, IBackgroundJobService backgroundJobs)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _backgroundJobs = backgroundJobs;
    }

    public async Task<IEnumerable<CourseResponse>> GetOpenCoursesAsync(int studentId)
    {

        var cacheKey = $"open-courses:{studentId}";

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
                .ThenInclude(c => c.PrerequisiteCourse)
            .Include(o => o.Enrollments)
            .Include(o => o.Semester)
            .Include(o => o.Instructor)
            .Where(o => o.Semester.IsActive)
            .ToListAsync();

        var completedEnrollments = await _unitOfWork.Repository<Enrollment>()
            .GetQueryable()
            .Where(e => e.StudentId == studentId && e.LetterGrade != null)
            .Include(e => e.CourseOffering)
            .ToListAsync();

        var completedCourseIds = completedEnrollments
            .Where(e => !e.LetterGrade!.Equals("FF", StringComparison.OrdinalIgnoreCase))
            .Select(e => e.CourseOffering.CourseId)
            .ToHashSet();

        var currentEnrollments = await _unitOfWork.Repository<Enrollment>()
            .GetQueryable()
            .Where(e => e.StudentId == studentId
                && e.IsActive
                && e.CourseOffering.Semester.IsActive)
            .Include(e => e.CourseOffering)
            .ToListAsync();

        var result = offerings
            .Where(o => !o.Course.PrerequisiteCourseId.HasValue
                || completedCourseIds.Contains(o.Course.PrerequisiteCourseId.Value))
            .Where(o => !currentEnrollments.Any(e =>
                SchedulesOverlap(e.CourseOffering, o)))
            .Where(o => o.EndTime > o.StartTime)
            .Select(o => new CourseResponse
            {
                Id = o.Id,
                Code = o.Course.Code,
                Name = o.Course.Name,
                Credits = o.Course.Credits,
                Quota = o.Course.Quota,
                AvailableSlots = o.Course.Quota - o.Enrollments.Count,
                HasPrerequisite = o.Course.PrerequisiteCourseId.HasValue,
                PrerequisiteCode = o.Course.PrerequisiteCourse?.Code,
                Day = o.Day.ToString(),
                StartTime = o.StartTime.ToString(@"hh\:mm"),
                EndTime = o.EndTime.ToString(@"hh\:mm"),
                Classroom = o.Classroom,
                InstructorName = $"{o.Instructor.FirstName} {o.Instructor.LastName}"
            }).ToList();
        await _cache.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(10)
        );
        return result;
    }
    public async Task<IEnumerable<EnrollmentResponse>> GetActiveEnrollmentsAsync(int studentId)
    {
        var cacheKey = $"active-enrollments:{studentId}";

        var cachedEnrollments =
            await _cache.GetAsync<List<EnrollmentResponse>>(cacheKey);

        if (cachedEnrollments is not null)
        {
            Console.WriteLine(
                $"REDIS HIT: {cacheKey} - {cachedEnrollments.Count} enrollments"
            );

            return cachedEnrollments;
        }
        var enrollments = await _unitOfWork.Repository<Enrollment>()
            .GetQueryable()
            .Include(e => e.CourseOffering)
                .ThenInclude(o => o.Course)
            .Include(e => e.CourseOffering)
                .ThenInclude(o => o.Instructor)
            .Include(e => e.CourseOffering)
                .ThenInclude(o => o.Semester)
            .Where(e => e.StudentId == studentId
                        && e.IsActive
                        && e.CourseOffering.Semester.IsActive)
            .ToListAsync();

        var result = enrollments.Select(e => new EnrollmentResponse
        {
            Id = e.Id,
            CourseOfferingId = e.CourseOfferingId,
            CourseCode = e.CourseOffering.Course.Code,
            CourseName = e.CourseOffering.Course.Name,
            Credits = e.CourseOffering.Course.Credits,
            LetterGrade = e.LetterGrade,
            GradePoint = e.GradePoint,
            Day = e.CourseOffering.Day.ToString(),
            StartTime = e.CourseOffering.StartTime.ToString(@"hh\:mm"),
            EndTime = e.CourseOffering.EndTime.ToString(@"hh\:mm"),
            Classroom = e.CourseOffering.Classroom,
            InstructorName = $"{e.CourseOffering.Instructor.FirstName} {e.CourseOffering.Instructor.LastName}",
        }).ToList();
        await _cache.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(10)
        );
        return result;
    }


    public async Task EnrollAsync(int studentId, int courseOfferingId)
    {


        var offeringRepo = _unitOfWork.Repository<CourseOffering>();
        var offering = await offeringRepo.GetQueryable()
            .Include(o => o.Course)
            .Include(o => o.Enrollments)
            .Include(o => o.Semester)
            .FirstOrDefaultAsync(o => o.Id == courseOfferingId);

        if (offering == null)
            throw new InvalidOperationException("Course offering not found.");

        if (offering.EndTime <= offering.StartTime)
            throw new InvalidOperationException("This course offering has an invalid schedule.");

        if (await _unitOfWork.Repository<Enrollment>().GetFirstAsync(
                e => e.StudentId == studentId
                    && e.CourseOfferingId == courseOfferingId
                    && e.IsActive) != null)
            throw new InvalidOperationException("You are already enrolled in this course.");

        if (DateTime.UtcNow > offering.Semester.RegistrationEnd)
            throw new InvalidOperationException("Registration period has ended.");
        else if (DateTime.UtcNow < offering.Semester.RegistrationStart)
            throw new InvalidOperationException("Registration period has not started yet.");
        // 2. Check Quota
        if (offering.Enrollments.Count >= offering.Course.Quota)
            throw new InvalidOperationException("Course quota is full.");

        // 3. Check Prerequisite
        if (offering.Course.PrerequisiteCourseId.HasValue)
        {
            var studentEnrollments = await _unitOfWork.Repository<Enrollment>()
                .GetQueryable()
                .Where(e => e.StudentId == studentId)
                .Include(e => e.CourseOffering)
                    .ThenInclude(o => o.Course)
                .ToListAsync();

            var hasPrereq = studentEnrollments.Any(e =>
                e.CourseOffering.Course.Id == offering.Course.PrerequisiteCourseId &&
                !string.IsNullOrWhiteSpace(e.LetterGrade) &&
                !e.LetterGrade.Equals("FF", StringComparison.OrdinalIgnoreCase));

            if (!hasPrereq)
                throw new InvalidOperationException("Prerequisite course not completed.");
        }

        // 4. Check Schedule Conflict (compare with the student's existing enrollments)
        var enrollmentRepo = _unitOfWork.Repository<Enrollment>();
        var existingEnrollments = await enrollmentRepo
            .GetQueryable()
            .Where(e => e.StudentId == studentId
                        && e.IsActive
                        && e.CourseOffering.SemesterId == offering.SemesterId)
            .Include(e => e.CourseOffering)
                .ThenInclude(o => o.Course)
            .ToListAsync();

        var hasConflict = existingEnrollments.Any(e =>
            SchedulesOverlap(e.CourseOffering, offering));

        if (hasConflict)
            throw new InvalidOperationException("Schedule conflict detected.");

        // 5. Check Max Credits (30 ECTS limit)
        var totalCredits = existingEnrollments
            .Where(e => e.CourseOffering.SemesterId == offering.SemesterId)
            .Sum(e => e.CourseOffering.Course.Credits);

        if (totalCredits + offering.Course.Credits > 30)
            throw new InvalidOperationException("Exceeds maximum credit limit (30 ECTS).");

        // 6. ✅ CREATE THE ENROLLMENT
        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseOfferingId = courseOfferingId,
            EnrollmentDate = DateTime.UtcNow,
            IsActive = true
        };

        await enrollmentRepo.AddAsync(enrollment);
        await _unitOfWork.SaveChangesAsync();
        await _cache.RemoveAsync($"active-enrollments:{studentId}");
        await _cache.RemoveAsync($"open-courses:{studentId}");

        var student = await _unitOfWork.Repository<User>().GetByIdAsync(studentId);
        var course = offering.Course;

        if (student == null)
            throw new InvalidOperationException("Student not found.");

        _backgroundJobs.Enqueue<IEmailService>(x => x.SendEmailAsync(
            student.Email,
            $"Enrollment Confirmation: {course.Code}",
            $"Dear {student.FirstName},\n\nYou have successfully enrolled in {course.Code} - {course.Name}.\n\nBest regards,\nUniversity Information System",
            false));
    }

    private static bool SchedulesOverlap(CourseOffering first, CourseOffering second)
    {
        if (first.Day != second.Day)
            return false;

        if (first.EndTime <= first.StartTime || second.EndTime <= second.StartTime)
            return true;

        return first.StartTime < second.EndTime
            && second.StartTime < first.EndTime;
    }
    public async Task DropAsync(int studentId, int enrollmentId)
    {
        var repo = _unitOfWork.Repository<Enrollment>();

        var enrollment = await repo.GetQueryable()
            .Include(e => e.CourseOffering)
                .ThenInclude(o => o.Semester)
            .Include(e => e.CourseOffering)
                .ThenInclude(o => o.Course)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.StudentId == studentId);

        if (enrollment == null)
            throw new Exception("Enrollment not found.");

        var studentEmail = (await _unitOfWork.Repository<User>().GetByIdAsync(studentId))?.Email
            ?? throw new InvalidOperationException("Student not found.");
        var studentFirstName = (await _unitOfWork.Repository<User>().GetByIdAsync(studentId))!.FirstName;
        var courseCode = enrollment.CourseOffering.Course.Code;
        var courseName = enrollment.CourseOffering.Course.Name;

        repo.Delete(enrollment);
        await _unitOfWork.SaveChangesAsync();

        await _cache.RemoveAsync($"active-enrollments:{studentId}");
        await _cache.RemoveAsync($"open-courses:{studentId}");

        _backgroundJobs.Enqueue<IEmailService>(x => x.SendEmailAsync(
            studentEmail,
            $"Withdrawal Confirmation: {courseCode}",
            $"Dear {studentFirstName},\n\nYou have successfully withdrawn from {courseCode} - {courseName}.\n\nBest regards,\nUniversity Information System",
            false));
    }
}