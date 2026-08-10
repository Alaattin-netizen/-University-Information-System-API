using UIS.Application.DTOs.Student.Courses;

namespace UIS.Application.Abstractions.StudentAbstractions;

public interface IEnrollmentService
{
    Task<IEnumerable<CourseResponse>> GetOpenCoursesAsync();
    Task EnrollAsync(int studentId, int courseOfferingId);
    Task DropAsync(int studentId, int enrollmentId);
}

public static class DiagnosticStudentAbstraction { }