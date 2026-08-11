using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Domain.Entities;
using UIS.Domain.Entities.Users;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.AdminServices;

public class FacultyService : IFacultyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly LoggingHelper _loggingHelper;

    public FacultyService(IUnitOfWork unitOfWork, LoggingHelper loggingHelper)
    {
        _unitOfWork = unitOfWork;
        _loggingHelper = loggingHelper;
    }

    // ======================================================
    // FACULTY CRUD
    // ======================================================

    public async Task<FacultyResponse> CreateFacultyAsync(CreateFacultyRequest request)
    {
        // Validate: Check for duplicate name
        var existing = await _unitOfWork.Repository<Faculty>()
            .GetFirstAsync(f => f.Name == request.Name);

        if (existing != null)
            throw new InvalidOperationException("Faculty already exists.");

        // Create
        var faculty = new Faculty
        {
            Name = request.Name,
            DeanName = request.DeanName
        };

        await _unitOfWork.Repository<Faculty>().AddAsync(faculty);
        await _unitOfWork.SaveChangesAsync();

        // Log
        await _loggingHelper.LogOperationAsync("Created", "Faculty", faculty.Id, $"Name: {faculty.Name}");

        // Return DTO
        return new FacultyResponse
        {
            Id = faculty.Id,
            Name = faculty.Name,
            DeanName = faculty.DeanName,
            DepartmentCount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    
    public async Task DeleteFacultyAsync(int id)
    {
        var faculty = await _unitOfWork.Repository<Faculty>()
            .GetQueryable()
            .Include(f => f.Departments)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (faculty == null)
            throw new InvalidOperationException("Faculty not found.");

        // Prevent deletion if departments exist
        if (faculty.Departments.Any())
            throw new InvalidOperationException("Cannot delete faculty with existing departments.");

        _unitOfWork.Repository<Faculty>().Delete(faculty);
        await _unitOfWork.SaveChangesAsync();

        // Log
        await _loggingHelper.LogOperationAsync("Deleted", "Faculty", id, $"Name: {faculty.Name}");
    }

    public async Task<IEnumerable<FacultyResponse>> GetAllFacultiesAsync()
    {
        var faculties = await _unitOfWork.Repository<Faculty>()
            .GetQueryable()
            .Include(f => f.Departments)
            .ToListAsync();

        return faculties.Select(f => new FacultyResponse
        {
            Id = f.Id,
            Name = f.Name,
            DeanName = f.DeanName,
            DepartmentCount = f.Departments?.Count ?? 0,
            CreatedAt = DateTime.UtcNow
        });
    }

    // ======================================================
    // DEPARTMENT CRUD
    // ======================================================

    public async Task<DepartmentResponse> CreateDepartmentAsync(CreateDepartmentRequest request)
    {
        // Validate: Faculty exists
        var faculty = await _unitOfWork.Repository<Faculty>().GetByIdAsync(request.FacultyId);
        if (faculty == null)
            throw new InvalidOperationException("Faculty not found.");

        // Validate: Duplicate department name within same faculty
        var existing = await _unitOfWork.Repository<Department>()
            .GetFirstAsync(d => d.Name == request.Name && d.FacultyId == request.FacultyId);

        if (existing != null)
            throw new InvalidOperationException("Department already exists in this faculty.");

        // Create
        var department = new Department
        {
            Name = request.Name,
            FacultyId = request.FacultyId
        };

        await _unitOfWork.Repository<Department>().AddAsync(department);
        await _unitOfWork.SaveChangesAsync();

        // Log
        await _loggingHelper.LogOperationAsync("Created", "Department", department.Id, $"Name: {department.Name}");

        return new DepartmentResponse
        {
            Id = department.Id,
            Name = department.Name,
            FacultyId = department.FacultyId,
            FacultyName = faculty.Name,
            StudentCount = 0,
            InstructorCount = 0,
            CourseCount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    

    public async Task DeleteDepartmentAsync(int id)
    {
        var department = await _unitOfWork.Repository<Department>()
            .GetQueryable()
            .Include(d => d.Students)
            .Include(d => d.Instructors)
            .Include(d => d.Courses)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
            throw new InvalidOperationException("Department not found.");

        // Prevent deletion if there are related records
        if (department.Students.Any() || department.Instructors.Any() || department.Courses.Any())
            throw new InvalidOperationException("Cannot delete department with existing students, instructors, or courses.");

        _unitOfWork.Repository<Department>().Delete(department);
        await _unitOfWork.SaveChangesAsync();

        // Log
        await _loggingHelper.LogOperationAsync("Deleted", "Department", id, $"Name: {department.Name}");
    }

    public async Task<IEnumerable<DepartmentResponse>> GetAllDepartmentsAsync()
    {
        var departments = await _unitOfWork.Repository<Department>()
            .GetQueryable()
            .Include(d => d.Faculty)
            .Include(d => d.Students)
            .Include(d => d.Instructors)
            .Include(d => d.Courses)
            .ToListAsync();

        return departments.Select(d => new DepartmentResponse
        {
            Id = d.Id,
            Name = d.Name,
            FacultyId = d.FacultyId,
            FacultyName = d.Faculty?.Name,
            StudentCount = d.Students?.Count ?? 0,
            InstructorCount = d.Instructors?.Count ?? 0,
            CourseCount = d.Courses?.Count ?? 0,
            CreatedAt = DateTime.UtcNow
        });
    }

    // ======================================================
    // COURSE CRUD
    // ======================================================

    public async Task<CourseResponse> CreateCourseAsync(CreateCourseRequest request)
    {
        // Validate: Department exists
        var department = await _unitOfWork.Repository<Department>().GetByIdAsync(request.DepartmentId);
        if (department == null)
            throw new InvalidOperationException("Department not found.");

        // Validate: Duplicate course code
        var existing = await _unitOfWork.Repository<Course>()
            .GetFirstAsync(c => c.Code == request.Code);

        if (existing != null)
            throw new InvalidOperationException("Course code already exists.");

        // Validate: Prerequisite exists
        if (request.PrerequisiteCourseId.HasValue)
        {
            var prereq = await _unitOfWork.Repository<Course>()
                .GetByIdAsync(request.PrerequisiteCourseId.Value);

            if (prereq == null)
                throw new InvalidOperationException("Prerequisite course not found.");
        }

        // Create
        var course = new Course
        {
            Code = request.Code,
            Name = request.Name,
            Credits = request.Credits,
            ECTS = request.ECTS,
            Quota = request.Quota,
            IsMandatory = request.IsMandatory,
            DepartmentId = request.DepartmentId,
            PrerequisiteCourseId = request.PrerequisiteCourseId
        };

        await _unitOfWork.Repository<Course>().AddAsync(course);
        await _unitOfWork.SaveChangesAsync();

        // Log
        await _loggingHelper.LogOperationAsync("Created", "Course", course.Id, $"Code: {course.Code}");

        return new CourseResponse
        {
            Id = course.Id,
            Code = course.Code,
            Name = course.Name,
            Credits = course.Credits,
            ECTS = course.ECTS,
            Quota = course.Quota,
            IsMandatory = course.IsMandatory,
            DepartmentId = course.DepartmentId,
            DepartmentName = department.Name,
            PrerequisiteCourseId = course.PrerequisiteCourseId,
            PrerequisiteCode = course.PrerequisiteCourse?.Code,
            OfferingCount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    
    public async Task DeleteCourseAsync(int id)
    {
        var course = await _unitOfWork.Repository<Course>()
            .GetQueryable()
            .Include(c => c.Offerings)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
            throw new InvalidOperationException("Course not found.");

        // Prevent deletion if there are offerings
        if (course.Offerings.Any())
            throw new InvalidOperationException("Cannot delete course with existing offerings.");

        _unitOfWork.Repository<Course>().Delete(course);
        await _unitOfWork.SaveChangesAsync();

        // Log
        await _loggingHelper.LogOperationAsync("Deleted", "Course", id, $"Code: {course.Code}");
    }

    public async Task<IEnumerable<CourseResponse>> GetAllCoursesAsync()
    {
        var courses = await _unitOfWork.Repository<Course>()
            .GetQueryable()
            .Include(c => c.Department)
            .Include(c => c.PrerequisiteCourse)
            .Include(c => c.Offerings)
            .ToListAsync();

        return courses.Select(c => new CourseResponse
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name,
            Credits = c.Credits,
            ECTS = c.ECTS,
            Quota = c.Quota,
            IsMandatory = c.IsMandatory,
            DepartmentId = c.DepartmentId,
            DepartmentName = c.Department?.Name,
            PrerequisiteCourseId = c.PrerequisiteCourseId,
            PrerequisiteCode = c.PrerequisiteCourse?.Code,
            OfferingCount = c.Offerings?.Count ?? 0,
            CreatedAt = DateTime.UtcNow
        });
    }
}