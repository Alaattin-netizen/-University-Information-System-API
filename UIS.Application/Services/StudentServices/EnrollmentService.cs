using Azure.Core;
using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.StudentAbstractions;
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

    public async Task EnrollAsync(int studentId, int courseOfferingId)
    {
        var offeringRepo = _unitOfWork.Repository<CourseOffering>();
        var offering = await offeringRepo.GetQueryable()
            .Include(o => o.Course)
            .Include(o => o.Enrollments)
            .FirstOrDefaultAsync(o => o.Id == courseOfferingId);

        if (offering == null)
            throw new Exception("Course offering not found.");

        if (offering.Enrollments.Count >= offering.Course.Quota)
            throw new Exception("Course quota is full.");

        if (offering.Course.PrerequisiteCourseId.HasValue)
        {
            // Query the Enrollment table directly to check if student has passed the prerequisite
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
                throw new Exception("Prerequisite course not completed.");
        }

        // ... rest of logic (schedule conflict, max credits, etc.)
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

        // Check if within registration period
        if (DateTime.UtcNow > enrollment.CourseOffering.Semester.RegistrationEnd)
            throw new Exception("Registration period has ended.");

        repo.Delete(enrollment);
        await _unitOfWork.SaveChangesAsync();
    }
}