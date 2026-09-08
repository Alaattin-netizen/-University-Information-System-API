using Azure.Core;
using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.StudentAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Application.DTOs.Student.Courses;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;
namespace UIS.Application.Services.StudentServices;

public class EnrollmentService : IEnrollmentService
{
    private readonly IUnitOfWork _unitOfWork;


    public EnrollmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CourseResponse>> GetOpenCoursesAsync(int studentId)
    {
        var offerings = await _unitOfWork.Repository<CourseOffering>()
            .GetQueryable()
            .Include(o => o.Course)              // ✅ MUST INCLUDE THIS
                .ThenInclude(c => c.PrerequisiteCourse)
            .Include(o => o.Enrollments)          // ✅ MUST INCLUDE THIS
            .Include(o => o.Semester)             // ✅ If you filter by Semester.IsActive
            .Include(o => o.Instructor)
            .Where(o => o.Semester.IsActive)      // Now Semester is loaded
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

        return offerings
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
            });
    }
    public async Task<IEnumerable<EnrollmentResponse>> GetActiveEnrollmentsAsync(int studentId)
    {
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

        return enrollments.Select(e => new EnrollmentResponse
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
            // Add any other properties you need (e.g., Midterm, Final, etc.)
        });
    }


    public async Task EnrollAsync(int studentId, int courseOfferingId)
    {


        // 1. Get the course offering with its Course and existing Enrollments
        var offeringRepo = _unitOfWork.Repository<CourseOffering>();
        var offering = await offeringRepo.GetQueryable()
            .Include(o => o.Course)
            .Include(o => o.Enrollments)
            .Include(o=> o.Semester)
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

        // ✅ Load CourseOffering and its Semester
        var enrollment = await repo.GetQueryable()
            .Include(e => e.CourseOffering)
                .ThenInclude(o => o.Semester)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.StudentId == studentId);

        if (enrollment == null)
            throw new Exception("Enrollment not found.");

       

        repo.Delete(enrollment);
        await _unitOfWork.SaveChangesAsync();
    }
}