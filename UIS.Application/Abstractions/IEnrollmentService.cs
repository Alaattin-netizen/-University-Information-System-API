using UIS.Application.DTOs.Courses;

namespace UIS.Application.Abstractions;

public interface IEnrollmentService
{
    Task<IEnumerable<CourseResponse>> GetOpenCoursesAsync();
    Task EnrollAsync(int studentId, int courseOfferingId);
    Task DropAsync(int studentId, int enrollmentId);
}