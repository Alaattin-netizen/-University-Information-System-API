using UIS.Application.Abstractions;
using UIS.Application.DTOs.Profile;
using UIS.Domain.Entities.Users;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services;

public class ProfileService : IProfileService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProfileService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task UpdateProfileAsync(int studentId, UpdateProfileRequest request)
    {
        var repo = _unitOfWork.Repository<Student>();
        var student = await repo.GetByIdAsync(studentId);

        if (student == null) throw new Exception("Student not found.");

        student.FirstName = request.FirstName;
        student.LastName = request.LastName;

        repo.Update(student);
        await _unitOfWork.SaveChangesAsync();
    }
}