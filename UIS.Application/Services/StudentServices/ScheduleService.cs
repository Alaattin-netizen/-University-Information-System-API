using Microsoft.EntityFrameworkCore;  // ✅ Required for .Include() and .ThenInclude()
using UIS.Application.Abstractions.StudentAbstractions;
using UIS.Application.DTOs.Student.Schedule;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.StudentServices;

public class ScheduleService : IScheduleService
{
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ScheduleResponse>> GetWeeklyScheduleAsync(int studentId)
    {
        // ✅ Include ALL navigation properties
        var enrollments = await _unitOfWork.Repository<Enrollment>()
            .GetQueryable()
            .Include(e => e.CourseOffering)              
                .ThenInclude(o => o.Course)              
            .Include(e => e.CourseOffering)              
                .ThenInclude(o => o.Instructor)        
            .Include(e => e.CourseOffering)             
                .ThenInclude(o => o.Semester)
            .Where(e => e.StudentId == studentId
                        && e.IsActive
                        && e.CourseOffering.Semester.IsActive) // Filter by active semester
            .ToListAsync();

        return enrollments
            .Select(e => new ScheduleResponse
            {
                Day = e.CourseOffering.Day.ToString(),
                StartTime = e.CourseOffering.StartTime.ToString(@"hh\:mm"),
                EndTime = e.CourseOffering.EndTime.ToString(@"hh\:mm"),
                CourseCode = e.CourseOffering.Course.Code,
                CourseName = e.CourseOffering.Course.Name,
                Instructor = $"{e.CourseOffering.Instructor.FirstName} {e.CourseOffering.Instructor.LastName}",
                Classroom = e.CourseOffering.Classroom
            })
            .OrderBy(s => s.Day)
            .ThenBy(s => s.StartTime)
            .ToList();
    }
}