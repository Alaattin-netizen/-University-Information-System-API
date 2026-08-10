using UIS.Application.DTOs.Profile;

namespace UIS.Application.Abstractions;

public interface IProfileService
{
    Task UpdateProfileAsync(int studentId, UpdateProfileRequest request);
}