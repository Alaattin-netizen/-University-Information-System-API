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

    public async Task<IEnumerable<CourseResponse>> GetOpenCoursesAsync()
    {
        var offerings = await _unitOfWork.Repository<CourseOffering>()
            .GetQueryable()
            .Include(o => o.Course)              // ✅ MUST INCLUDE THIS
            .Include(o => o.Enrollments)          // ✅ MUST INCLUDE THIS
            .Include(o => o.Semester)             // ✅ If you filter by Semester.IsActive
            .Where(o => o.Semester.IsActive)      // Now Semester is loaded
            .ToListAsync();

        return offerings.Select(o => new CourseResponse
        {
            Id = o.Id,
            Code = o.Course.Code,
            Name = o.Course.Name,
            Credits = o.Course.Credits,
            Quota = o.Course.Quota,
            AvailableSlots = o.Course.Quota - o.Enrollments.Count,
            HasPrerequisite = o.Course.PrerequisiteCourseId.HasValue,
            PrerequisiteCode = o.Course.PrerequisiteCourse?.Code
        });
    }
    public async Task<IEnumerable<EnrollmentResponse>> GetActiveEnrollmentsAsync(int studentId)
    {
        var enrollments = await _unitOfWork.Repository<Enrollment>()
            .GetQueryable()
            .Include(e => e.CourseOffering)
                .ThenInclude(o => o.Course)
            .Where(e => e.StudentId == studentId && e.IsActive)
            .ToListAsync();

        return enrollments.Select(e => new EnrollmentResponse
        {
            Id = e.Id,
            CourseCode = e.CourseOffering.Course.Code,
            CourseName = e.CourseOffering.Course.Name,
            Credits = e.CourseOffering.Course.Credits,
            LetterGrade = e.LetterGrade,
            GradePoint = e.GradePoint,
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
            throw new Exception("Course offering not found.");

        if (DateTime.UtcNow > offering.Semester.RegistrationEnd)
            throw new InvalidOperationException("Registration period has ended.");
        else if (DateTime.UtcNow < offering.Semester.RegistrationStart)
            throw new InvalidOperationException("Registration period has not started yet.");
        // 2. Check Quota
        if (offering.Enrollments.Count >= offering.Course.Quota)
            throw new Exception("Course quota is full.");

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
                e.LetterGrade != "FF" && e.LetterGrade != "DD");

            if (!hasPrereq)
                throw new InvalidOperationException("Prerequisite course not completed.");
        }

        // 4. Check Schedule Conflict (compare with the student's existing enrollments)
        var enrollmentRepo = _unitOfWork.Repository<Enrollment>();
        var existingEnrollments = await enrollmentRepo
            .GetQueryable()
            .Where(e => e.StudentId == studentId && e.IsActive)
            .Include(e => e.CourseOffering)
            .ToListAsync();

        var hasConflict = existingEnrollments.Any(e =>
            e.CourseOffering.Day == offering.Day &&
            ((e.CourseOffering.StartTime <= offering.StartTime && offering.StartTime < e.CourseOffering.EndTime) ||
             (e.CourseOffering.StartTime < offering.EndTime && offering.EndTime <= e.CourseOffering.EndTime)));

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