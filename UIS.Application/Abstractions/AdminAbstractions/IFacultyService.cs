using UIS.Application.DTOs.Admin.Course;
using UIS.Application.DTOs.Admin.Department;
using UIS.Application.DTOs.Admin.Faculty;

public interface IFacultyService
{
    // Faculty
    Task<FacultyResponse> CreateFacultyAsync(CreateFacultyRequest request);
    Task<FacultyResponse> UpdateFacultyAsync(UpdateFacultyRequest request);
    Task DeleteFacultyAsync(int id);
    Task<FacultyResponse> GetFacultyByIdAsync(int id);
    Task<IEnumerable<FacultyResponse>> GetAllFacultiesAsync(); // ✅ Added

    // Department
    Task<DepartmentResponse> CreateDepartmentAsync(CreateDepartmentRequest request);
    Task<DepartmentResponse> UpdateDepartmentAsync(UpdateDepartmentRequest request);
    Task DeleteDepartmentAsync(int id);
    Task<DepartmentResponse> GetDepartmentByIdAsync(int id);
    Task<IEnumerable<DepartmentResponse>> GetAllDepartmentsAsync(); // ✅ Added

    // Course
    Task<CourseResponse> CreateCourseAsync(CreateCourseRequest request);
    Task<CourseResponse> UpdateCourseAsync(UpdateCourseRequest request);
    Task DeleteCourseAsync(int id);
    Task<CourseResponse> GetCourseByIdAsync(int id);
    Task<IEnumerable<CourseResponse>> GetAllCoursesAsync(); // ✅ Added
}