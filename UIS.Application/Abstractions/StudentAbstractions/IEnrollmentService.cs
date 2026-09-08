using UIS.Application.DTOs.Admin;
using UIS.Application.DTOs.Student.Courses;

namespace UIS.Application.Abstractions.StudentAbstractions;

public interface IEnrollmentService
{
    Task<IEnumerable<CourseResponse>> GetOpenCoursesAsync(int studentId);
    Task EnrollAsync(int studentId, int courseOfferingId);
    Task DropAsync(int studentId, int enrollmentId);
    Task<IEnumerable<EnrollmentResponse>> GetActiveEnrollmentsAsync(int studentId);

}

public static class DiagnosticStudentAbstraction { }