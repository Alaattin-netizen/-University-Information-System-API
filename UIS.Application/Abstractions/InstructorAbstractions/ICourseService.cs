using UIS.Application.DTOs.Instructor;

namespace UIS.Application.Abstractions.InstructorAbstractions;

public interface ICourseService  // ✅ MUST be public
{
    Task<IEnumerable<CourseResponse>> GetMyCoursesAsync(int instructorId);
    Task<IEnumerable<RegisteredStudentResponse>> GetRegisteredStudentsAsync(int instructorId, int courseOfferingId);
    Task CreateAnnouncementAsync(int instructorId, CreateAnnouncementRequest request);
}