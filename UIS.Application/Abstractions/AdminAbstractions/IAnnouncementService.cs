using UIS.Application.DTOs.Admin;

namespace UIS.Application.Abstractions.AdminAbstractions;

public interface IAnnouncementService
{
    Task<AnnouncementResponse> CreateAsync(CreateAnnouncementRequest request);
    Task<AnnouncementResponse> UpdateAsync(UpdateAnnouncementRequest request);
    Task DeleteAsync(int id);
    Task<AnnouncementResponse> GetByIdAsync(int id);
    Task<IEnumerable<AnnouncementResponse>> GetAllAsync();
}