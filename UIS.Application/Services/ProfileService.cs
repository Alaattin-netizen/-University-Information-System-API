using UIS.Application.Abstractions;
using UIS.Application.DTOs.Profile;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services;

public class ProfileService : IProfileService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProfileService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var repo = _unitOfWork.Repository<User>();
        var user = await repo.GetByIdAsync(userId);

        if (user == null) throw new Exception("User not found.");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        repo.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }
}