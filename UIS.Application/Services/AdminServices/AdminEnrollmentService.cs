using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.AdminServices;

public class AdminEnrollmentService : IAdminEnrollmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminEnrollmentService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<EnrollmentResponse> CreateAsync(CreateEnrollmentRequest request)
    {
        var student = await _unitOfWork.Repository<User>().GetByIdAsync(request.StudentId);
        if (student == null) throw new InvalidOperationException("Student not found.");

        var offering = await _unitOfWork.Repository<CourseOffering>()
            .GetQueryable()
            .Include(o => o.Course)
            .Include(o => o.Enrollments)
            .FirstOrDefaultAsync(o => o.Id == request.CourseOfferingId);

        if (offering == null) throw new InvalidOperationException("Course offering not found.");
        if (offering.Enrollments.Count >= offering.Course.Quota) throw new InvalidOperationException("Course quota is full.");

        var existing = await _unitOfWork.Repository<Enrollment>()
            .GetFirstAsync(e => e.StudentId == request.StudentId && e.CourseOfferingId == request.CourseOfferingId);
        if (existing != null) throw new InvalidOperationException("Student already enrolled.");

        var enrollment = new Enrollment
        {
            StudentId = request.StudentId,
            CourseOfferingId = request.CourseOfferingId,
            EnrollmentDate = DateTime.UtcNow,
            IsActive = true
        };

        await _unitOfWork.Repository<Enrollment>().AddAsync(enrollment);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(enrollment.Id);
    }

    public async Task<EnrollmentResponse> UpdateAsync(UpdateEnrollmentRequest request)
    {
        var enrollment = await _unitOfWork.Repository<Enrollment>()
            .GetQueryable()
            .Include(e => e.Student)
            .Include(e => e.CourseOffering).ThenInclude(o => o.Course)
            .FirstOrDefaultAsync(e => e.Id == request.Id);

        if (enrollment == null) throw new InvalidOperationException("Enrollment not found.");

        if (request.MidtermScore.HasValue) enrollment.MidtermScore = request.MidtermScore.Value;
        if (request.FinalScore.HasValue) enrollment.FinalScore = request.FinalScore.Value;
        if (request.TotalScore.HasValue) enrollment.TotalScore = request.TotalScore.Value;
        if (!string.IsNullOrEmpty(request.LetterGrade)) enrollment.LetterGrade = request.LetterGrade;
        if (request.GradePoint.HasValue) enrollment.GradePoint = request.GradePoint.Value;

        _unitOfWork.Repository<Enrollment>().Update(enrollment);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(enrollment.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var enrollment = await _unitOfWork.Repository<Enrollment>().GetByIdAsync(id);
        if (enrollment == null) throw new InvalidOperationException("Enrollment not found.");
        _unitOfWork.Repository<Enrollment>().Delete(enrollment);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<EnrollmentResponse> GetByIdAsync(int id)
    {
        var e = await _unitOfWork.Repository<Enrollment>()
            .GetQueryable()
            .Include(e => e.Student)
            .Include(e => e.CourseOffering).ThenInclude(o => o.Course)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (e == null) throw new InvalidOperationException("Enrollment not found.");
        return MapToResponse(e);
    }

    public async Task<IEnumerable<EnrollmentResponse>> GetAllAsync()
    {
        var list = await _unitOfWork.Repository<Enrollment>()
            .GetQueryable()
            .Include(e => e.Student)
            .Include(e => e.CourseOffering).ThenInclude(o => o.Course)
            .OrderByDescending(e => e.EnrollmentDate)
            .ToListAsync();

        return list.Select(MapToResponse);
    }

    public async Task<IEnumerable<EnrollmentResponse>> GetByStudentAsync(int studentId)
    {
        var list = await _unitOfWork.Repository<Enrollment>()
            .GetQueryable()
            .Include(e => e.Student)
            .Include(e => e.CourseOffering).ThenInclude(o => o.Course)
            .Where(e => e.StudentId == studentId)
            .ToListAsync();

        return list.Select(MapToResponse);
    }

    private EnrollmentResponse MapToResponse(Enrollment e) => new()
    {
        Id = e.Id,
        StudentId = e.StudentId,
        StudentName = $"{e.Student.FirstName} {e.Student.LastName}",
        CourseOfferingId = e.CourseOfferingId,
        CourseCode = e.CourseOffering?.Course?.Code,
        EnrollmentDate = e.EnrollmentDate,
        IsActive = e.IsActive,
        MidtermScore = e.MidtermScore,
        FinalScore = e.FinalScore,
        TotalScore = e.TotalScore,
        LetterGrade = e.LetterGrade,
        GradePoint = e.GradePoint
    };
}