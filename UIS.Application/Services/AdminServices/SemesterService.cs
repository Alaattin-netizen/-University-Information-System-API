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
    // REQUIRED INTERFACE METHODS
    // ======================================================

    public async Task<int> OpenSemesterAsync(CreateSemesterRequest request)
    {
        // Validate: Check for duplicate name
        var existing = await _unitOfWork.Repository<Semester>()
            .GetFirstAsync(s => s.Name == request.Name);

        if (existing != null)
            throw new InvalidOperationException("Semester with this name already exists.");

        // If this semester is active, deactivate all others
        if (request.IsActive)
        {
            var allSemesters = await _unitOfWork.Repository<Semester>().GetAllAsync();
            foreach (var s in allSemesters)
            {
                s.IsActive = false;
                _unitOfWork.Repository<Semester>().Update(s);
            }
        }

        // Create
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

        // Log
        return semester.Id;
    }

    public async Task<int> UpdateRegistrationCalenderAsync(int semesterId, UpdateSemesterRequest request)
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

        // Update
   
        semester.RegistrationStart = request.RegistrationStart;
        semester.RegistrationEnd = request.RegistrationEnd;
        semester.IsActive = request.IsActive;

        _unitOfWork.Repository<Semester>().Update(semester);
        await _unitOfWork.SaveChangesAsync();

        // Log
        return semester.Id;
    }

  

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
}