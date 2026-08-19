using UIS.Application.DTOs.Admin.Semester;
using UIS.Application.DTOs.Filters;

public interface ISemesterService
{
    Task<int> OpenSemesterAsync(CreateSemesterRequest request);
    Task<SemesterResponse> CreateSemesterAsync(CreateSemesterRequest request);
    Task<SemesterResponse> UpdateSemesterAsync(UpdateSemesterRequest request);
    Task DeleteSemesterAsync(int id);
    Task<SemesterResponse> GetSemesterByIdAsync(int id);
    Task<IEnumerable<SemesterResponse>> GetAllSemestersAsync(SemesterFilterRequest? filter=null);
    Task<SemesterResponse> UpdateRegistrationCalendarAsync(int semesterId, UpdateRegistrationDateRequest request); // ✅ Changed parameter type
}