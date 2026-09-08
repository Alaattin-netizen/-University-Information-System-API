using UIS.Application.DTOs.Instructor;
using UIS.Application.DTOs.Admin;

namespace UIS.Application.Abstractions.InstructorAbstractions;

public interface ICourseService  // ✅ MUST be public
{
    Task<IEnumerable<CourseResponse>> GetMyCoursesAsync(int instructorId);
    Task<IEnumerable<CourseResponse>> GetMyCoursesForDateAsync(int instructorId, DateTime date);
    Task<IEnumerable<RegisteredStudentResponse>> GetRegisteredStudentsAsync(int instructorId, int courseOfferingId);
    Task CreateAnnouncementAsync(int instructorId, CreateAnnouncementRequest request);
    Task<IEnumerable<AnnouncementResponse>> GetAnnouncementsAsync(int instructorId, int courseOfferingId);
}