using UIS.Application.DTOs.Grades;

namespace UIS.Application.Abstractions;

public interface IGradeService
{
    Task<IEnumerable<GradeResponse>> GetGradesAsync(int studentId);
    Task<GPAResponse> GetGPAAsync(int studentId);
    Task<IEnumerable<GradeResponse>> GetTranscriptAsync(int studentId);
}