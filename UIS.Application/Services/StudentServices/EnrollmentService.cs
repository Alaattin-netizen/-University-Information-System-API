using Azure.Core;
using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.StudentAbstractions;
using UIS.Application.DTOs.Student.Courses;
using UIS.Domain.Entities;
using UIS.Domain.Entities.Users;
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

        // 🔥 Load CourseOffering with its related Course and Enrollments
        var offering = await offeringRepo.GetQueryable()
            .Include(o => o.Course)
            .Include(o => o.Enrollments)
            .FirstOrDefaultAsync(o => o.Id == courseOfferingId);

        if (offering == null)
            throw new Exception("Course offering not found.");

        // 1. Check Quota
        if (offering.Enrollments.Count >= offering.Course.Quota)
            throw new Exception("Course quota is full.");

        // 2. Check Prerequisite
        if (offering.Course.PrerequisiteCourseId.HasValue)
        {
            var studentRepo = _unitOfWork.Repository<Student>();
            // Load student with enrollments and their courses for prerequisite check
            var student = await studentRepo.GetQueryable()
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.CourseOffering)
                        .ThenInclude(o => o.Course)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                throw new Exception("Student not found.");

            var hasPrereq = student.Enrollments.Any(e =>
                e.CourseOffering.Course.Id == offering.Course.PrerequisiteCourseId &&
                e.LetterGrade != "FF" && e.LetterGrade != "DD");

            if (!hasPrereq)
                throw new Exception("Prerequisite course not completed.");
        }

        // 3. Check Schedule Conflict
        var enrollmentRepo = _unitOfWork.Repository<Enrollment>();
        var existingEnrollments = await enrollmentRepo.GetQueryable()
            .Include(e => e.CourseOffering)
            .Where(e => e.StudentId == studentId &&
                        e.CourseOffering.SemesterId == offering.SemesterId &&
                        e.IsActive)
            .ToListAsync();

        var hasConflict = existingEnrollments.Any(e =>
            e.CourseOffering.Day == offering.Day &&
            ((e.CourseOffering.StartTime <= offering.StartTime && offering.StartTime < e.CourseOffering.EndTime) ||
             (e.CourseOffering.StartTime < offering.EndTime && offering.EndTime <= e.CourseOffering.EndTime)));

        if (hasConflict)
            throw new Exception("Schedule conflict detected.");

        // 4. Check Max Credits
        var totalCredits = existingEnrollments.Sum(e => e.CourseOffering.Course.Credits);
        if (totalCredits + offering.Course.Credits > 30)
            throw new Exception("Exceeds maximum credit limit (30 ECTS).");

        // 5. Enroll
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

        // Check if within registration period
        if (DateTime.UtcNow > enrollment.CourseOffering.Semester.RegistrationEnd)
            throw new Exception("Registration period has ended.");

        repo.Delete(enrollment);
        await _unitOfWork.SaveChangesAsync();
    }
}