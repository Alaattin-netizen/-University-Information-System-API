using System;
using System.Collections.Generic;
using System.Text;
using UIS.Application.DTOs.Admin;


namespace UIS.Application.Abstractions.AdminAbstractions
{
    public interface IFacultyService
    {
        Task<FacultyResponse> CreateFacultyAsync(CreateFacultyRequest request);
        Task<DepartmentResponse> CreateDepartmentAsync(int facultyId, CreateDepartmentRequest request);

        Task<CourseResponse> CreateCourseAsync(int departmentId, CreateCourseRequest request);

    }
}
