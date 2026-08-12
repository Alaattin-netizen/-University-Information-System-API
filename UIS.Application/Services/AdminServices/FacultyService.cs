using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin.Course;
using UIS.Application.DTOs.Admin.Department;
using UIS.Application.DTOs.Admin.Faculty;
using UIS.Domain.Entities;
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
        var existing = await _unitOfWork.Repository<Faculty>()
            .GetFirstAsync(f => f.Name == request.Name);

        if (existing != null)
            throw new InvalidOperationException("Faculty already exists.");

        var faculty = new Faculty
        {
            Name = request.Name,
            DeanName = request.DeanName
        };

        await _unitOfWork.Repository<Faculty>().AddAsync(faculty);
        await _unitOfWork.SaveChangesAsync();


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

        if (faculty.Departments.Any())
            throw new InvalidOperationException("Cannot delete faculty with existing departments.");

        _unitOfWork.Repository<Faculty>().Delete(faculty);
        await _unitOfWork.SaveChangesAsync();

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
        var faculty = await _unitOfWork.Repository<Faculty>().GetByIdAsync(request.FacultyId);
        if (faculty == null)
            throw new InvalidOperationException("Faculty not found.");

        var existing = await _unitOfWork.Repository<Department>()
            .GetFirstAsync(d => d.Name == request.Name && d.FacultyId == request.FacultyId);

        if (existing != null)
            throw new InvalidOperationException("Department already exists in this faculty.");

        var department = new Department
        {
            Name = request.Name,
            FacultyId = request.FacultyId
        };

        await _unitOfWork.Repository<Department>().AddAsync(department);
        await _unitOfWork.SaveChangesAsync();


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
            .Include(d => d.Users) // ✅ Changed from Students & Instructors to Users
            .Include(d => d.Courses)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
            throw new InvalidOperationException("Department not found.");

        // Check if there are any users (students or instructors) in this department
        if (department.Users.Any())
            throw new InvalidOperationException("Cannot delete department with existing users.");

        if (department.Courses.Any())
            throw new InvalidOperationException("Cannot delete department with existing courses.");

        _unitOfWork.Repository<Department>().Delete(department);
        await _unitOfWork.SaveChangesAsync();

    }

    public async Task<IEnumerable<DepartmentResponse>> GetAllDepartmentsAsync()
    {
        var departments = await _unitOfWork.Repository<Department>()
            .GetQueryable()
            .Include(d => d.Faculty)
            .Include(d => d.Users) // ✅ Changed from Students & Instructors to Users
            .Include(d => d.Courses)
            .ToListAsync();

        return departments.Select(d => new DepartmentResponse
        {
            Id = d.Id,
            Name = d.Name,
            FacultyId = d.FacultyId,
            FacultyName = d.Faculty?.Name,
            StudentCount = d.Users.Count(u => u.UserRoles.Any(ur => ur.Role.Name == "Student")),
            InstructorCount = d.Users.Count(u => u.UserRoles.Any(ur => ur.Role.Name == "Instructor")),
            CourseCount = d.Courses?.Count ?? 0,
            CreatedAt = DateTime.UtcNow
        });
    }

    // ======================================================
    // COURSE CRUD
    // ======================================================

    public async Task<CourseResponse> CreateCourseAsync(CreateCourseRequest request)
    {
        var department = await _unitOfWork.Repository<Department>().GetByIdAsync(request.DepartmentId);
        if (department == null)
            throw new InvalidOperationException("Department not found.");

        var existing = await _unitOfWork.Repository<Course>()
            .GetFirstAsync(c => c.Code == request.Code);

        if (existing != null)
            throw new InvalidOperationException("Course code already exists.");

        if (request.PrerequisiteCourseId.HasValue)
        {
            var prereq = await _unitOfWork.Repository<Course>()
                .GetByIdAsync(request.PrerequisiteCourseId.Value);

            if (prereq == null)
                throw new InvalidOperationException("Prerequisite course not found.");
        }

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

        if (course.Offerings.Any())
            throw new InvalidOperationException("Cannot delete course with existing offerings.");

        _unitOfWork.Repository<Course>().Delete(course);
        await _unitOfWork.SaveChangesAsync();

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
    // ======================================================
    // FACULTY: GetById & Update
    // ======================================================

    public async Task<FacultyResponse> GetFacultyByIdAsync(int id)
    {
        var faculty = await _unitOfWork.Repository<Faculty>()
            .GetQueryable()
            .Include(f => f.Departments)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (faculty == null)
            throw new InvalidOperationException("Faculty not found.");

        return new FacultyResponse
        {
            Id = faculty.Id,
            Name = faculty.Name,
            DeanName = faculty.DeanName,
            DepartmentCount = faculty.Departments?.Count ?? 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<FacultyResponse> UpdateFacultyAsync(UpdateFacultyRequest request)
    {
        var faculty = await _unitOfWork.Repository<Faculty>()
            .GetQueryable()
            .Include(f => f.Departments)
            .FirstOrDefaultAsync(f => f.Id == request.Id);

        if (faculty == null)
            throw new InvalidOperationException("Faculty not found.");

        // Check duplicate name (excluding itself)
        var duplicate = await _unitOfWork.Repository<Faculty>()
            .GetFirstAsync(f => f.Name == request.Name && f.Id != request.Id);

        if (duplicate != null)
            throw new InvalidOperationException("Another faculty with this name already exists.");

        faculty.Name = request.Name;
        faculty.DeanName = request.DeanName;

        _unitOfWork.Repository<Faculty>().Update(faculty);
        await _unitOfWork.SaveChangesAsync();

      
        return new FacultyResponse
        {
            Id = faculty.Id,
            Name = faculty.Name,
            DeanName = faculty.DeanName,
            DepartmentCount = faculty.Departments?.Count ?? 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ======================================================
    // DEPARTMENT: GetById & Update
    // ======================================================

    public async Task<DepartmentResponse> GetDepartmentByIdAsync(int id)
    {
        var department = await _unitOfWork.Repository<Department>()
            .GetQueryable()
            .Include(d => d.Faculty)
            .Include(d => d.Users)
            .Include(d => d.Courses)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
            throw new InvalidOperationException("Department not found.");

        return new DepartmentResponse
        {
            Id = department.Id,
            Name = department.Name,
            FacultyId = department.FacultyId,
            FacultyName = department.Faculty?.Name,
            StudentCount = department.Users.Count(u => u.UserRoles.Any(ur => ur.Role.Name == "Student")),
            InstructorCount = department.Users.Count(u => u.UserRoles.Any(ur => ur.Role.Name == "Instructor")),
            CourseCount = department.Courses?.Count ?? 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<DepartmentResponse> UpdateDepartmentAsync(UpdateDepartmentRequest request)
    {
        var department = await _unitOfWork.Repository<Department>()
            .GetQueryable()
            .Include(d => d.Faculty)
            .Include(d => d.Users)
            .Include(d => d.Courses)
            .FirstOrDefaultAsync(d => d.Id == request.Id);

        if (department == null)
            throw new InvalidOperationException("Department not found.");

        // Check duplicate name within same faculty (excluding itself)
        var duplicate = await _unitOfWork.Repository<Department>()
            .GetFirstAsync(d => d.Name == request.Name && d.FacultyId == request.FacultyId && d.Id != request.Id);

        if (duplicate != null)
            throw new InvalidOperationException("Another department with this name already exists in this faculty.");

        department.Name = request.Name;
        department.FacultyId = request.FacultyId;

        _unitOfWork.Repository<Department>().Update(department);
        await _unitOfWork.SaveChangesAsync();

 

        return new DepartmentResponse
        {
            Id = department.Id,
            Name = department.Name,
            FacultyId = department.FacultyId,
            FacultyName = department.Faculty?.Name,
            StudentCount = department.Users.Count(u => u.UserRoles.Any(ur => ur.Role.Name == "Student")),
            InstructorCount = department.Users.Count(u => u.UserRoles.Any(ur => ur.Role.Name == "Instructor")),
            CourseCount = department.Courses?.Count ?? 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ======================================================
    // COURSE: GetById & Update
    // ======================================================

    public async Task<CourseResponse> GetCourseByIdAsync(int id)
    {
        var course = await _unitOfWork.Repository<Course>()
            .GetQueryable()
            .Include(c => c.Department)
            .Include(c => c.PrerequisiteCourse)
            .Include(c => c.Offerings)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
            throw new InvalidOperationException("Course not found.");

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
            DepartmentName = course.Department?.Name,
            PrerequisiteCourseId = course.PrerequisiteCourseId,
            PrerequisiteCode = course.PrerequisiteCourse?.Code,
            OfferingCount = course.Offerings?.Count ?? 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<CourseResponse> UpdateCourseAsync(UpdateCourseRequest request)
    {
        var course = await _unitOfWork.Repository<Course>()
            .GetQueryable()
            .Include(c => c.Department)
            .Include(c => c.PrerequisiteCourse)
            .Include(c => c.Offerings)
            .FirstOrDefaultAsync(c => c.Id == request.Id);

        if (course == null)
            throw new InvalidOperationException("Course not found.");

        // Validate: Department exists
        var department = await _unitOfWork.Repository<Department>().GetByIdAsync(request.DepartmentId);
        if (department == null)
            throw new InvalidOperationException("Department not found.");

        // Check duplicate code (excluding itself)
        var duplicate = await _unitOfWork.Repository<Course>()
            .GetFirstAsync(c => c.Code == request.Code && c.Id != request.Id);

        if (duplicate != null)
            throw new InvalidOperationException("Another course with this code already exists.");

        // Validate: Prerequisite exists
        if (request.PrerequisiteCourseId.HasValue)
        {
            var prereq = await _unitOfWork.Repository<Course>()
                .GetByIdAsync(request.PrerequisiteCourseId.Value);

            if (prereq == null)
                throw new InvalidOperationException("Prerequisite course not found.");
        }

        course.Code = request.Code;
        course.Name = request.Name;
        course.Credits = request.Credits;
        course.ECTS = request.ECTS;
        course.Quota = request.Quota;
        course.IsMandatory = request.IsMandatory;
        course.DepartmentId = request.DepartmentId;
        course.PrerequisiteCourseId = request.PrerequisiteCourseId;

        _unitOfWork.Repository<Course>().Update(course);
        await _unitOfWork.SaveChangesAsync();

     
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
            OfferingCount = course.Offerings?.Count ?? 0,
            CreatedAt = DateTime.UtcNow
        };
    }
}