using UIS.Application.DTOs.Admin;
using UIS.Application.DTOs.Admin.Enrollment;
using UIS.Domain.Entities;

namespace UIS.Application.Abstractions.AdminAbstractions;

public interface IAdminEnrollmentService
{
    Task<EnrollmentResponse> CreateAsync(CreateEnrollmentRequest request);
    Task<EnrollmentResponse> UpdateAsync(UpdateEnrollmentRequest request);
    Task DeleteAsync(int id);
    Task<EnrollmentResponse> GetByIdAsync(int id);
    Task<IEnumerable<EnrollmentResponse>> GetAllAsync(EnrollmentFilterRequest? filter=null);
    Task<IEnumerable<EnrollmentResponse>> GetByStudentAsync(int studentId);
}