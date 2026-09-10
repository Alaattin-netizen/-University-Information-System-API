using UIS.Application.DTOs.Admin;
using UIS.Application.DTOs.Admin.Attendance;
using UIS.Application.DTOs.Filters;

namespace UIS.Application.Abstractions.AdminAbstractions;

public interface IAttendanceService
{
    Task<AttendanceResponse> CreateAsync(CreateAttendanceRequest request);
    Task<AttendanceResponse> UpdateAsync(UpdateAttendanceRequest request);
    Task DeleteAsync(int id);
    Task<AttendanceResponse> GetByIdAsync(int id);
    Task<IEnumerable<AttendanceResponse>> GetAllAsync(AttendanceFilterRequest? filter = null, int? instructorId = null);
    Task<IEnumerable<AttendanceResponse>> GetByStudentAsync(int studentId);
    Task<IEnumerable<AttendanceResponse>> GetByCourseOfferingAsync(int courseOfferingId);
    Task<byte[]> ExportAsync(AttendanceFilterRequest? filter = null, int? instructorId = null);
    Task<AttendanceImportResult> ImportAsync(Stream file, int? instructorId = null);
}