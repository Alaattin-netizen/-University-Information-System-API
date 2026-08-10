using UIS.Application.DTOs.Student.Grades;

namespace UIS.Application.Abstractions.StudentAbstractions;

public interface IGradeService
{
    Task<IEnumerable<GradeResponse>> GetGradesAsync(int studentId);
    Task<GPAResponse> GetGPAAsync(int studentId);
    Task<IEnumerable<GradeResponse>> GetTranscriptAsync(int studentId);
}