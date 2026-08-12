using UIS.Application.DTOs.Admin;

namespace UIS.Application.Abstractions.AdminAbstractions;

public interface IAdminEnrollmentService
{
    Task<EnrollmentResponse> CreateAsync(CreateEnrollmentRequest request);
    Task<EnrollmentResponse> UpdateAsync(UpdateEnrollmentRequest request);
    Task DeleteAsync(int id);
    Task<EnrollmentResponse> GetByIdAsync(int id);
    Task<IEnumerable<EnrollmentResponse>> GetAllAsync();
    Task<IEnumerable<EnrollmentResponse>> GetByStudentAsync(int studentId);
}