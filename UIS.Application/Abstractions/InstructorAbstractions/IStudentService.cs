using UIS.Application.DTOs.Instructor;

namespace UIS.Application.Abstractions.InstructorAbstractions;

public interface IStudentService  // ✅ MUST be public
{
    Task EnterGradesAsync(int instructorId, GradeEntryRequest request);
    Task EnterAttendanceAsync(int instructorId, AttendanceEntryRequest request);
}