using UIS.Application.DTOs.Schedule;

namespace UIS.Application.Abstractions;

public interface IScheduleService
{
    Task<IEnumerable<ScheduleResponse>> GetWeeklyScheduleAsync(int studentId);
}