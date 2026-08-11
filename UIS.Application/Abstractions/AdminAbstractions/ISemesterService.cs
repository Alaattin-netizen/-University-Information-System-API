using System;
using System.Collections.Generic;
using System.Text;
using UIS.Application.DTOs.Admin;

namespace UIS.Application.Abstractions.AdminAbstractions
{
    public interface ISemesterService
    {

      Task<int> OpenSemesterAsync(CreateSemesterRequest request);
      Task<int> UpdateRegistrationCalenderAsync(int semesterId, UpdateSemesterRequest request);
    }
}
