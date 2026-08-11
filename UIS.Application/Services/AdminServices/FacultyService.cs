using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;
using UIS.Application.DTOs.Admin;


namespace UIS.Application.Services.AdminServices
{
    public class FacultyService : IFacultyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Helper _helper; // ✅ Inject Helper

        public FacultyService(IUnitOfWork unitOfWork, Helper helper)
        {
            _unitOfWork = unitOfWork;
            _helper = helper; // ✅
        }

        public async Task<FacultyResponse> CreateFacultyAsync(CreateFacultyRequest request)
        {
            var faculty = new Faculty
            {
                Name = request.Name,
                DeanName = request.DeanName
            };

            await _unitOfWork.Repository<Faculty>().AddAsync(faculty);
            await _unitOfWork.SaveChangesAsync();

            // ✅ Now call the Helper method
            await _helper.LogOperationAsync("Created", "Faculty", faculty.Id, $"Name: {faculty.Name}");

            return new FacultyResponse
            {
                Id = faculty.Id,
                Name = faculty.Name,
                DeanName = faculty.DeanName,
                DepartmentCount = 0,
                CreatedAt = faculty.CreatedDate // Make sure this property exists in Faculty entity
            };
        }

        public async Task<DepartmentResponse> CreateDepartmentAsync(int facultyId, CreateDepartmentRequest request)
        {
            var department = new Department
            {
                Name = request.Name,
                FacultyId = facultyId
            };

            await _unitOfWork.Repository<Department>().AddAsync(department);
            await _unitOfWork.SaveChangesAsync();

            // ✅ Now call the Helper method
            await _helper.LogOperationAsync("Created", "Department", department.Id, $"Name: {department.Name}");

            return new DepartmentResponse
            {
                Id = department.Id,
                Name = department.Name,
                FacultyId = department.FacultyId
            };
        }

        public async Task<CourseResponse> CreateCourseAsync(int departmentId, CreateCourseRequest request)
        {
            var course = new Course
            {
                Name = request.Name,
                DepartmentId = departmentId
            };

            await _unitOfWork.Repository<Course>().AddAsync(course);
            await _unitOfWork.SaveChangesAsync();

            // ✅ Now call the Helper method
            await _helper.LogOperationAsync("Created", "Course", course.Id, $"Name: {course.Name}");

            return new CourseResponse
            {
                Id = course.Id,
                Name = course.Name,
                DepartmentId = course.DepartmentId
            };
        }




    }
}