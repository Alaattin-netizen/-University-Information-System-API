using System;
using System.Collections.Generic;
using System.Text;
using UIS.Application.DTOs.Admin;

namespace UIS.Application.Abstractions.AdminAbstractions
{
    public interface IUserService
    {
        Task<UserResponse> CreateStudentAsync(CreateStudentRequest request);

        Task<UserResponse> CreateInstructorAsync(CreateInstructorRequest request);

    }
}
