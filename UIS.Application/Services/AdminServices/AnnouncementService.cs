using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.AdminServices;

public class AnnouncementService : IAnnouncementService
{
    private readonly IUnitOfWork _unitOfWork;

    public AnnouncementService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<AnnouncementResponse> CreateAsync(CreateAnnouncementRequest request)
    {
        var offering = await _unitOfWork.Repository<CourseOffering>().GetByIdAsync(request.CourseOfferingId);
        if (offering == null) throw new InvalidOperationException("Course offering not found.");

        var announcement = new Announcement
        {
            Title = request.Title,
            Content = request.Content,
            CreatedDate = DateTime.UtcNow,
            CourseOfferingId = request.CourseOfferingId,
            InstructorId = offering.InstructorId
        };

        await _unitOfWork.Repository<Announcement>().AddAsync(announcement);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(announcement.Id);
    }

    public async Task<AnnouncementResponse> UpdateAsync(UpdateAnnouncementRequest request)
    {
        var announcement = await _unitOfWork.Repository<Announcement>()
            .GetQueryable()
            .Include(a => a.CourseOffering).ThenInclude(o => o.Course)
            .Include(a => a.Instructor)
            .FirstOrDefaultAsync(a => a.Id == request.Id);

        if (announcement == null) throw new InvalidOperationException("Announcement not found.");

        announcement.Title = request.Title;
        announcement.Content = request.Content;

        _unitOfWork.Repository<Announcement>().Update(announcement);
        await _unitOfWork.SaveChangesAsync();
        return MapToResponse(announcement);
    }

    public async Task DeleteAsync(int id)
    {
        var announcement = await _unitOfWork.Repository<Announcement>().GetByIdAsync(id);
        if (announcement == null) throw new InvalidOperationException("Announcement not found.");
        _unitOfWork.Repository<Announcement>().Delete(announcement);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<AnnouncementResponse> GetByIdAsync(int id)
    {
        var a = await _unitOfWork.Repository<Announcement>()
            .GetQueryable()
            .Include(a => a.CourseOffering).ThenInclude(o => o.Course)
            .Include(a => a.Instructor)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (a == null) throw new InvalidOperationException("Announcement not found.");
        return MapToResponse(a);
    }

    public async Task<IEnumerable<AnnouncementResponse>> GetAllAsync()
    {
        var list = await _unitOfWork.Repository<Announcement>()
            .GetQueryable()
            .Include(a => a.CourseOffering).ThenInclude(o => o.Course)
            .Include(a => a.Instructor)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();

        return list.Select(MapToResponse);
    }

    private AnnouncementResponse MapToResponse(Announcement a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        Content = a.Content,
        CreatedDate = a.CreatedDate,
        CourseOfferingId = a.CourseOfferingId,
        InstructorId = a.InstructorId,
        InstructorName = $"{a.Instructor.FirstName} {a.Instructor.LastName}",
        CourseCode = a.CourseOffering?.Course?.Code
    };
}