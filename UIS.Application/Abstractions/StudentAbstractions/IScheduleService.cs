using UIS.Application.DTOs.Student.Schedule;

namespace UIS.Application.Abstractions.StudentAbstractions;

public interface IScheduleService
{
    Task<IEnumerable<ScheduleResponse>> GetWeeklyScheduleAsync(int studentId);
}