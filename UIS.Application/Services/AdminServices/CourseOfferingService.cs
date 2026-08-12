using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.AdminServices;

public class CourseOfferingService : ICourseOfferingService
{
    private readonly IUnitOfWork _unitOfWork;

    public CourseOfferingService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<CourseOfferingResponse> CreateAsync(CreateCourseOfferingRequest request)
    {
        var course = await _unitOfWork.Repository<Course>().GetByIdAsync(request.CourseId);
        if (course == null) throw new InvalidOperationException("Course not found.");
        var instructor = await _unitOfWork.Repository<User>().GetByIdAsync(request.InstructorId);
        if (instructor == null) throw new InvalidOperationException("Instructor not found.");
        var semester = await _unitOfWork.Repository<Semester>().GetByIdAsync(request.SemesterId);
        if (semester == null) throw new InvalidOperationException("Semester not found.");

        var offering = new CourseOffering
        {
            CourseId = request.CourseId,
            InstructorId = request.InstructorId,
            SemesterId = request.SemesterId,
            Day = (DayOfWeek)request.Day,
            StartTime = TimeSpan.Parse(request.StartTime),
            EndTime = TimeSpan.Parse(request.EndTime),
            Classroom = request.Classroom
        };

        await _unitOfWork.Repository<CourseOffering>().AddAsync(offering);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(offering.Id);
    }

    public async Task<CourseOfferingResponse> UpdateAsync(UpdateCourseOfferingRequest request)
    {
        var offering = await _unitOfWork.Repository<CourseOffering>()
            .GetQueryable()
            .Include(o => o.Course)
            .Include(o => o.Instructor)
            .Include(o => o.Semester)
            .FirstOrDefaultAsync(o => o.Id == request.Id);

        if (offering == null) throw new InvalidOperationException("Course offering not found.");

        if (request.InstructorId.HasValue)
        {
            var instructor = await _unitOfWork.Repository<User>().GetByIdAsync(request.InstructorId.Value);
            if (instructor == null) throw new InvalidOperationException("Instructor not found.");
            offering.InstructorId = request.InstructorId.Value;
        }
        if (request.SemesterId.HasValue)
        {
            var semester = await _unitOfWork.Repository<Semester>().GetByIdAsync(request.SemesterId.Value);
            if (semester == null) throw new InvalidOperationException("Semester not found.");
            offering.SemesterId = request.SemesterId.Value;
        }
        if (request.Day.HasValue) offering.Day = (DayOfWeek)request.Day.Value;
        if (!string.IsNullOrEmpty(request.StartTime)) offering.StartTime = TimeSpan.Parse(request.StartTime);
        if (!string.IsNullOrEmpty(request.EndTime)) offering.EndTime = TimeSpan.Parse(request.EndTime);
        if (!string.IsNullOrEmpty(request.Classroom)) offering.Classroom = request.Classroom;

        _unitOfWork.Repository<CourseOffering>().Update(offering);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(offering.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var offering = await _unitOfWork.Repository<CourseOffering>()
            .GetQueryable()
            .Include(o => o.Enrollments)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (offering == null) throw new InvalidOperationException("Course offering not found.");
        if (offering.Enrollments.Any()) throw new InvalidOperationException("Cannot delete course offering with existing enrollments.");

        _unitOfWork.Repository<CourseOffering>().Delete(offering);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<CourseOfferingResponse> GetByIdAsync(int id)
    {
        var o = await _unitOfWork.Repository<CourseOffering>()
            .GetQueryable()
            .Include(o => o.Course)
            .Include(o => o.Instructor)
            .Include(o => o.Semester)
            .Include(o => o.Enrollments)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (o == null) throw new InvalidOperationException("Course offering not found.");
        return MapToResponse(o);
    }

    public async Task<IEnumerable<CourseOfferingResponse>> GetAllAsync()
    {
        var list = await _unitOfWork.Repository<CourseOffering>()
            .GetQueryable()
            .Include(o => o.Course)
            .Include(o => o.Instructor)
            .Include(o => o.Semester)
            .Include(o => o.Enrollments)
            .OrderBy(o => o.SemesterId)
            .ThenBy(o => o.Day)
            .ToListAsync();

        return list.Select(MapToResponse);
    }

    private CourseOfferingResponse MapToResponse(CourseOffering o) => new()
    {
        Id = o.Id,
        CourseId = o.CourseId,
        CourseCode = o.Course?.Code,
        CourseName = o.Course?.Name,
        InstructorId = o.InstructorId,
        InstructorName = $"{o.Instructor.FirstName} {o.Instructor.LastName}",
        SemesterId = o.SemesterId,
        SemesterName = o.Semester?.Name,
        Day = o.Day.ToString(),
        StartTime = o.StartTime.ToString(@"hh\:mm"),
        EndTime = o.EndTime.ToString(@"hh\:mm"),
        Classroom = o.Classroom,
        EnrolledCount = o.Enrollments?.Count ?? 0
    };
}