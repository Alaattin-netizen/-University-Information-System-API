using UIS.Domain.Entities;
using UIS.Domain.Entities.Users;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.AdminServices;

public class UserCreationService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserCreationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> CreateStudentAsync(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        int? departmentId,
        int? advisorId)
    {
        var student = new Student
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = passwordHash,
            Role = "Student",
            DepartmentId = departmentId,
            AdvisorId = advisorId
        };

        await _unitOfWork.Repository<Student>().AddAsync(student);
        await _unitOfWork.SaveChangesAsync();

        return student.Id;
    }

    public async Task<int> CreateInstructorAsync(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        int? departmentId)
    {
        var instructor = new Instructor
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = passwordHash,
            Role = "Instructor",
            DepartmentId = departmentId
        };

        await _unitOfWork.Repository<Instructor>().AddAsync(instructor);
        await _unitOfWork.SaveChangesAsync();

        return instructor.Id;
    }

    public async Task<int> CreateAdminAsync(
        string firstName,
        string lastName,
        string email,
        string passwordHash)
    {
        var admin = new Admin
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = passwordHash,
            Role = "Admin"
        };

        await _unitOfWork.Repository<Admin>().AddAsync(admin);
        await _unitOfWork.SaveChangesAsync();

        return admin.Id;
    }
}