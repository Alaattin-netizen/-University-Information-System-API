using UIS.Application.DTOs.Student.Profile;

namespace UIS.Application.Abstractions.StudentAbstractions;

public interface IProfileService
{
    Task UpdateProfileAsync(int studentId, UpdateProfileRequest request);
}