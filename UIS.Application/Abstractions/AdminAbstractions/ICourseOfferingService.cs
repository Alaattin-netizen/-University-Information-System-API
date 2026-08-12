using UIS.Application.DTOs.Admin;

namespace UIS.Application.Abstractions.AdminAbstractions;

public interface ICourseOfferingService
{
    Task<CourseOfferingResponse> CreateAsync(CreateCourseOfferingRequest request);
    Task<CourseOfferingResponse> UpdateAsync(UpdateCourseOfferingRequest request);
    Task DeleteAsync(int id);
    Task<CourseOfferingResponse> GetByIdAsync(int id);
    Task<IEnumerable<CourseOfferingResponse>> GetAllAsync();
}