using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.AdminServices;

public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;

    public AttendanceService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<AttendanceResponse> CreateAsync(CreateAttendanceRequest request)
    {
        var student = await _unitOfWork.Repository<User>().GetByIdAsync(request.StudentId);
        if (student == null) throw new InvalidOperationException("Student not found.");
        var offering = await _unitOfWork.Repository<CourseOffering>().GetByIdAsync(request.CourseOfferingId);
        if (offering == null) throw new InvalidOperationException("Course offering not found.");

        var attendance = new Attendance
        {
            StudentId = request.StudentId,
            CourseOfferingId = request.CourseOfferingId,
            Date = request.Date,
            IsPresent = request.IsPresent
        };

        await _unitOfWork.Repository<Attendance>().AddAsync(attendance);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(attendance.Id);
    }

    public async Task<AttendanceResponse> UpdateAsync(UpdateAttendanceRequest request)
    {
        var attendance = await _unitOfWork.Repository<Attendance>()
            .GetQueryable()
            .Include(a => a.Student)
            .Include(a => a.CourseOffering).ThenInclude(o => o.Course)
            .FirstOrDefaultAsync(a => a.Id == request.Id);

        if (attendance == null) throw new InvalidOperationException("Attendance record not found.");

        attendance.IsPresent = request.IsPresent;
        _unitOfWork.Repository<Attendance>().Update(attendance);
        await _unitOfWork.SaveChangesAsync();
        return MapToResponse(attendance);
    }

    public async Task DeleteAsync(int id)
    {
        var attendance = await _unitOfWork.Repository<Attendance>().GetByIdAsync(id);
        if (attendance == null) throw new InvalidOperationException("Attendance record not found.");
        _unitOfWork.Repository<Attendance>().Delete(attendance);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<AttendanceResponse> GetByIdAsync(int id)
    {
        var a = await _unitOfWork.Repository<Attendance>()
            .GetQueryable()
            .Include(a => a.Student)
            .Include(a => a.CourseOffering).ThenInclude(o => o.Course)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (a == null) throw new InvalidOperationException("Attendance record not found.");
        return MapToResponse(a);
    }

    public async Task<IEnumerable<AttendanceResponse>> GetAllAsync()
    {
        var list = await _unitOfWork.Repository<Attendance>()
            .GetQueryable()
            .Include(a => a.Student)
            .Include(a => a.CourseOffering).ThenInclude(o => o.Course)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        return list.Select(MapToResponse);
    }

    public async Task<IEnumerable<AttendanceResponse>> GetByStudentAsync(int studentId)
    {
        var list = await _unitOfWork.Repository<Attendance>()
            .GetQueryable()
            .Include(a => a.Student)
            .Include(a => a.CourseOffering).ThenInclude(o => o.Course)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        return list.Select(MapToResponse);
    }

    public async Task<IEnumerable<AttendanceResponse>> GetByCourseOfferingAsync(int courseOfferingId)
    {
        var list = await _unitOfWork.Repository<Attendance>()
            .GetQueryable()
            .Include(a => a.Student)
            .Include(a => a.CourseOffering).ThenInclude(o => o.Course)
            .Where(a => a.CourseOfferingId == courseOfferingId)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        return list.Select(MapToResponse);
    }

    private AttendanceResponse MapToResponse(Attendance a) => new()
    {
        Id = a.Id,
        StudentId = a.StudentId,
        StudentName = $"{a.Student.FirstName} {a.Student.LastName}",
        CourseOfferingId = a.CourseOfferingId,
        CourseCode = a.CourseOffering?.Course?.Code,
        Date = a.Date,
        IsPresent = a.IsPresent
    };
}