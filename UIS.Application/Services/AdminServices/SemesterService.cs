using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.AdminServices;

public class SemesterService : ISemesterService
{
    private readonly IUnitOfWork _unitOfWork;

    public SemesterService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ======================================================
    // OPEN SEMESTER (Returns ID - for backward compatibility)
    // ======================================================

    public async Task<int> OpenSemesterAsync(CreateSemesterRequest request)
    {
        var existing = await _unitOfWork.Repository<Semester>()
            .GetFirstAsync(s => s.Name == request.Name);

        if (existing != null)
            throw new InvalidOperationException("Semester with this name already exists.");

        if (request.IsActive)
        {
            var allSemesters = await _unitOfWork.Repository<Semester>().GetAllAsync();
            foreach (var s in allSemesters)
            {
                s.IsActive = false;
                _unitOfWork.Repository<Semester>().Update(s);
            }
        }

        var semester = new Semester
        {
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RegistrationStart = request.RegistrationStart,
            RegistrationEnd = request.RegistrationEnd,
            IsActive = request.IsActive
        };

        await _unitOfWork.Repository<Semester>().AddAsync(semester);
        await _unitOfWork.SaveChangesAsync();

        return semester.Id;
    }

    // ======================================================
    // CREATE SEMESTER (Returns full response)
    // ======================================================

    public async Task<SemesterResponse> CreateSemesterAsync(CreateSemesterRequest request)
    {
        var existing = await _unitOfWork.Repository<Semester>()
            .GetFirstAsync(s => s.Name == request.Name);

        if (existing != null)
            throw new InvalidOperationException("Semester with this name already exists.");

        if (request.IsActive)
        {
            var allSemesters = await _unitOfWork.Repository<Semester>().GetAllAsync();
            foreach (var s in allSemesters)
            {
                s.IsActive = false;
                _unitOfWork.Repository<Semester>().Update(s);
            }
        }

        var semester = new Semester
        {
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RegistrationStart = request.RegistrationStart,
            RegistrationEnd = request.RegistrationEnd,
            IsActive = request.IsActive
        };

        await _unitOfWork.Repository<Semester>().AddAsync(semester);
        await _unitOfWork.SaveChangesAsync();

        return new SemesterResponse
        {
            Id = semester.Id,
            Name = semester.Name,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate,
            RegistrationStart = semester.RegistrationStart,
            RegistrationEnd = semester.RegistrationEnd,
            IsActive = semester.IsActive,
            CourseOfferingCount = 0,
            EnrollmentCount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ======================================================
    // UPDATE SEMESTER
    // ======================================================

    public async Task<SemesterResponse> UpdateSemesterAsync(UpdateSemesterRequest request)
    {
        var semester = await _unitOfWork.Repository<Semester>()
            .GetQueryable()
            .Include(s => s.CourseOfferings)
                .ThenInclude(o => o.Enrollments)
            .FirstOrDefaultAsync(s => s.Id == request.Id);

        if (semester == null)
            throw new InvalidOperationException("Semester not found.");

        var duplicate = await _unitOfWork.Repository<Semester>()
            .GetFirstAsync(s => s.Name == request.Name && s.Id != request.Id);

        if (duplicate != null)
            throw new InvalidOperationException("Another semester with this name already exists.");

        if (request.IsActive && !semester.IsActive)
        {
            var allSemesters = await _unitOfWork.Repository<Semester>().GetAllAsync();
            foreach (var s in allSemesters)
            {
                if (s.Id != request.Id)
                {
                    s.IsActive = false;
                    _unitOfWork.Repository<Semester>().Update(s);
                }
            }
        }

        semester.Name = request.Name;
        semester.StartDate = request.StartDate;
        semester.EndDate = request.EndDate;
        semester.RegistrationStart = request.RegistrationStart;
        semester.RegistrationEnd = request.RegistrationEnd;
        semester.IsActive = request.IsActive;

        _unitOfWork.Repository<Semester>().Update(semester);
        await _unitOfWork.SaveChangesAsync();

        var enrollmentCount = semester.CourseOfferings?.Sum(o => o.Enrollments?.Count ?? 0) ?? 0;

        return new SemesterResponse
        {
            Id = semester.Id,
            Name = semester.Name,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate,
            RegistrationStart = semester.RegistrationStart,
            RegistrationEnd = semester.RegistrationEnd,
            IsActive = semester.IsActive,
            CourseOfferingCount = semester.CourseOfferings?.Count ?? 0,
            EnrollmentCount = enrollmentCount,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ======================================================
    // UPDATE REGISTRATION CALENDAR (✅ Correct spelling)
    // ======================================================

    public async Task<SemesterResponse> UpdateRegistrationCalendarAsync(int semesterId, UpdateRegistrationDateRequest request)
    {
        var semester = await _unitOfWork.Repository<Semester>().GetByIdAsync(semesterId);
        if (semester == null)
            throw new InvalidOperationException("Semester not found.");

        // If this semester is being activated, deactivate all others
        if (request.IsActive && !semester.IsActive)
        {
            var allSemesters = await _unitOfWork.Repository<Semester>().GetAllAsync();
            foreach (var s in allSemesters)
            {
                if (s.Id != semesterId)
                {
                    s.IsActive = false;
                    _unitOfWork.Repository<Semester>().Update(s);
                }
            }
        }

        semester.RegistrationStart = request.RegistrationStart;
        semester.RegistrationEnd = request.RegistrationEnd;
        semester.IsActive = request.IsActive;

        _unitOfWork.Repository<Semester>().Update(semester);
        await _unitOfWork.SaveChangesAsync();

        // Return updated semester
        return await GetSemesterByIdAsync(semesterId);
    }

    // ======================================================
    // DELETE SEMESTER
    // ======================================================

    public async Task DeleteSemesterAsync(int id)
    {
        var semester = await _unitOfWork.Repository<Semester>()
            .GetQueryable()
            .Include(s => s.CourseOfferings)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (semester == null)
            throw new InvalidOperationException("Semester not found.");

        if (semester.CourseOfferings.Any())
            throw new InvalidOperationException("Cannot delete semester with existing course offerings.");

        _unitOfWork.Repository<Semester>().Delete(semester);
        await _unitOfWork.SaveChangesAsync();
    }

    // ======================================================
    // GET SEMESTER BY ID
    // ======================================================

    public async Task<SemesterResponse> GetSemesterByIdAsync(int id)
    {
        var semester = await _unitOfWork.Repository<Semester>()
            .GetQueryable()
            .Include(s => s.CourseOfferings)
                .ThenInclude(o => o.Enrollments)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (semester == null)
            throw new InvalidOperationException("Semester not found.");

        var enrollmentCount = semester.CourseOfferings?.Sum(o => o.Enrollments?.Count ?? 0) ?? 0;

        return new SemesterResponse
        {
            Id = semester.Id,
            Name = semester.Name,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate,
            RegistrationStart = semester.RegistrationStart,
            RegistrationEnd = semester.RegistrationEnd,
            IsActive = semester.IsActive,
            CourseOfferingCount = semester.CourseOfferings?.Count ?? 0,
            EnrollmentCount = enrollmentCount,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ======================================================
    // GET ALL SEMESTERS
    // ======================================================

    public async Task<IEnumerable<SemesterResponse>> GetAllSemestersAsync()
    {
        var semesters = await _unitOfWork.Repository<Semester>()
            .GetQueryable()
            .Include(s => s.CourseOfferings)
                .ThenInclude(o => o.Enrollments)
            .ToListAsync();

        return semesters.Select(s => new SemesterResponse
        {
            Id = s.Id,
            Name = s.Name,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            RegistrationStart = s.RegistrationStart,
            RegistrationEnd = s.RegistrationEnd,
            IsActive = s.IsActive,
            CourseOfferingCount = s.CourseOfferings?.Count ?? 0,
            EnrollmentCount = s.CourseOfferings?.Sum(o => o.Enrollments?.Count ?? 0) ?? 0,
            CreatedAt = DateTime.UtcNow
        });
    }
}