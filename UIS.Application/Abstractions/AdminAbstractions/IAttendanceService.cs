using UIS.Application.DTOs.Admin;

namespace UIS.Application.Abstractions.AdminAbstractions;

public interface IAttendanceService
{
    Task<AttendanceResponse> CreateAsync(CreateAttendanceRequest request);
    Task<AttendanceResponse> UpdateAsync(UpdateAttendanceRequest request);
    Task DeleteAsync(int id);
    Task<AttendanceResponse> GetByIdAsync(int id);
    Task<IEnumerable<AttendanceResponse>> GetAllAsync();
    Task<IEnumerable<AttendanceResponse>> GetByStudentAsync(int studentId);
    Task<IEnumerable<AttendanceResponse>> GetByCourseOfferingAsync(int courseOfferingId);
}