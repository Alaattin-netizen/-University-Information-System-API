using UIS.Application.Abstractions;
using UIS.Application.DTOs.Courses;
using UIS.Domain.Entities;
using UIS.Domain.Entities.Users;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public EnrollmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CourseResponse>> GetOpenCoursesAsync()
    {
        var repo = _unitOfWork.Repository<CourseOffering>();
        var offerings = await repo.FindAsync(o => o.Semester.IsActive);

        return offerings.Select(o => new CourseResponse
        {
            Id = o.CourseId,
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
        var enrollmentRepo = _unitOfWork.Repository<Enrollment>();

        var offering = await offeringRepo.GetByIdAsync(courseOfferingId);
        if (offering == null) throw new Exception("Course offering not found.");

        // 1. Check Quota
        if (offering.Enrollments.Count >= offering.Course.Quota)
            throw new Exception("Course quota is full.");

        // 2. Check Prerequisite
        if (offering.Course.PrerequisiteCourseId.HasValue)
        {
            var studentRepo = _unitOfWork.Repository<Student>();
            var student = await studentRepo.GetByIdAsync(studentId);

            var hasPrereq = student.Enrollments.Any(e =>
                e.CourseOffering.Course.PrerequisiteCourseId == offering.Course.PrerequisiteCourseId &&
                e.LetterGrade != "FF" && e.LetterGrade != "DD"); // Passing grade

            if (!hasPrereq)
                throw new Exception("Prerequisite course not completed.");
        }

        // 3. Check Schedule Conflict
        var existingEnrollments = await enrollmentRepo.FindAsync(e =>
            e.StudentId == studentId &&
            e.CourseOffering.SemesterId == offering.SemesterId);

        var hasConflict = existingEnrollments.Any(e =>
            e.CourseOffering.Day == offering.Day &&
            ((e.CourseOffering.StartTime <= offering.StartTime && offering.StartTime < e.CourseOffering.EndTime) ||
             (e.CourseOffering.StartTime < offering.EndTime && offering.EndTime <= e.CourseOffering.EndTime)));

        if (hasConflict) throw new Exception("Schedule conflict detected.");

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
        var enrollment = await repo.GetFirstAsync(e => e.Id == enrollmentId && e.StudentId == studentId);

        if (enrollment == null) throw new Exception("Enrollment not found.");

        // Check if within registration period
        if (DateTime.UtcNow > enrollment.CourseOffering.Semester.RegistrationEnd)
            throw new Exception("Registration period has ended.");

        repo.Delete(enrollment);
        await _unitOfWork.SaveChangesAsync();
    }
}