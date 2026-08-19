using UIS.Application.DTOs.Admin;
using UIS.Application.DTOs.Filters;

namespace UIS.Application.Abstractions.AdminAbstractions;

public interface IAnnouncementService
{
    Task<AnnouncementResponse> CreateAsync(AdminCreateAnnouncementRequest request);
    Task<AnnouncementResponse> UpdateAsync(UpdateAnnouncementRequest request);
    Task DeleteAsync(int id);
    Task<AnnouncementResponse> GetByIdAsync(int id);
    Task<IEnumerable<AnnouncementResponse>> GetAllAsync(AnnouncementFilterRequest? filter=null);
}