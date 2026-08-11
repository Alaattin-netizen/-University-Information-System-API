using System;
using System.Collections.Generic;
using System.Text;
using UIS.Application.DTOs.Admin;

namespace UIS.Application.Abstractions.AdminAbstractions
{
    public interface ISemesterService
    {

      Task OpenSemesterAsync(CreateSemesterRequest request);
      Task UpdateRegistrationCalenderAsync(int semesterId, UpdateSemesterRequest request);
    }
}
