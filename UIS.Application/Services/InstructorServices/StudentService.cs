using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.InstructorAbstractions;
using UIS.Application.DTOs.Instructor;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.InstructorServices;

public class StudentService : IStudentService
{
    private readonly IUnitOfWork _unitOfWork;

    public StudentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // 3. Enter/Update grades for a student
    public async Task EnterGradesAsync(int instructorId, GradeEntryRequest request)
    {
        var enrollment = await _unitOfWork.Repository<Enrollment>()
            .GetQueryable()
            .Include(e => e.CourseOffering)
                .ThenInclude(o => o.Course)
            .FirstOrDefaultAsync(e => e.Id == request.EnrollmentId);

        if (enrollment == null)
            throw new Exception("Enrollment not found.");

        // Verify the instructor owns this course
        if (enrollment.CourseOffering.InstructorId != instructorId)
            throw new Exception("You don't have permission to grade this student.");

        // Update scores
        enrollment.MidtermScore = request.MidtermScore;
        enrollment.FinalScore = request.FinalScore;
        enrollment.AssignmentScore = request.AssignmentScore;
        enrollment.MakeupScore = request.MakeupScore;

        // Calculate weighted total
        double totalScore = 0;
        var hasValidGrade = false;

        if (enrollment.MidtermScore.HasValue && enrollment.FinalScore.HasValue)
        {
            totalScore = (enrollment.MidtermScore.Value * 0.4) + (enrollment.FinalScore.Value * 0.6);
            hasValidGrade = true;
        }
        else if (enrollment.MakeupScore.HasValue)
        {
            totalScore = enrollment.MakeupScore.Value;
            hasValidGrade = true;
        }

        if (hasValidGrade)
        {
            totalScore = Math.Round(totalScore, 2);
            enrollment.TotalScore = totalScore;

            var (letterGrade, gradePoint) = GetGradeInfo(totalScore);
            enrollment.LetterGrade = letterGrade;
            enrollment.GradePoint = gradePoint;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    // 4. Enter attendance for a student
    public async Task EnterAttendanceAsync(int instructorId, AttendanceEntryRequest request)
    {
        // Find the course offering taught by this instructor
        var offering = await _unitOfWork.Repository<CourseOffering>().GetQueryable()
            .FirstOrDefaultAsync(o => o.InstructorId == instructorId);

        if (offering == null)
            throw new Exception("You don't have permission to enter attendance for this course.");

        // Check if student is enrolled in this course
        var enrollment = await _unitOfWork.Repository<Enrollment>().GetQueryable()
            .FirstOrDefaultAsync(e => e.StudentId == request.StudentId &&
                                      e.CourseOfferingId == offering.Id &&
                                      e.IsActive);

        if (enrollment == null)
            throw new Exception("Student is not enrolled in this course.");

        // Check if attendance already exists for this date
        var existingAttendance = await _unitOfWork.Repository<Attendance>()
            .GetFirstAsync(a => a.StudentId == request.StudentId &&
                                a.CourseOfferingId == offering.Id &&
                                a.Date.Date == request.Date.Date);

        if (existingAttendance != null)
        {
            existingAttendance.IsPresent = request.IsPresent;
            _unitOfWork.Repository<Attendance>().Update(existingAttendance);
        }
        else
        {
            var attendance = new Attendance
            {
                StudentId = request.StudentId,
                CourseOfferingId = offering.Id,
                Date = request.Date.Date,
                IsPresent = request.IsPresent
            };
            await _unitOfWork.Repository<Attendance>().AddAsync(attendance);
        }

        await _unitOfWork.SaveChangesAsync();
    }
    private (string LetterGrade, double GradePoint) GetGradeInfo(double score)
    {
        if (score >= 90) return ("AA", 4.0);
        if (score >= 85) return ("BA", 3.5);
        if (score >= 80) return ("BB", 3.0);
        if (score >= 75) return ("CB", 2.5);
        if (score >= 70) return ("CC", 2.0);
        if (score >= 60) return ("DC", 1.5);
        if (score >= 50) return ("DD", 1.0);
        return ("FF", 0.0);
    }
}

