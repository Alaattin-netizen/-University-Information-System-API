using UIS.Application.Abstractions;
using UIS.Application.DTOs.Schedule;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services;

public class ScheduleService : IScheduleService
{
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ScheduleResponse>> GetWeeklyScheduleAsync(int studentId)
    {
        var repo = _unitOfWork.Repository<Enrollment>();
        var enrollments = await repo.FindAsync(e =>
            e.StudentId == studentId &&
            e.IsActive &&
            e.CourseOffering.Semester.IsActive);

        return enrollments.Select(e => new ScheduleResponse
        {
            Day = e.CourseOffering.Day.ToString(),
            StartTime = e.CourseOffering.StartTime.ToString(@"hh\:mm"),
            EndTime = e.CourseOffering.EndTime.ToString(@"hh\:mm"),
            CourseCode = e.CourseOffering.Course.Code,
            CourseName = e.CourseOffering.Course.Name,
            Instructor = $"{e.CourseOffering.Instructor.FirstName} {e.CourseOffering.Instructor.LastName}",
            Classroom = e.CourseOffering.Classroom
        }).OrderBy(s => s.Day).ThenBy(s => s.StartTime);
    }
}