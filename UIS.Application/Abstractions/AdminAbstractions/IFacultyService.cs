using System;
using System.Collections.Generic;
using System.Text;
using UIS.Application.DTOs.Admin;


namespace UIS.Application.Abstractions.AdminAbstractions
{
    public interface IFacultyService
    {
        Task<FacultyResponse> CreateFacultyAsync(CreateFacultyRequest request);
        Task<DepartmentResponse> CreateDepartmentAsync( CreateDepartmentRequest request);

        Task<CourseResponse> CreateCourseAsync( CreateCourseRequest request);

    }
}
