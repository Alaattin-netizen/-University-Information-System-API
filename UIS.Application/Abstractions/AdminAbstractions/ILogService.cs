using System;
using System.Collections.Generic;
using System.Text;
using UIS.Application.DTOs.Admin;

namespace UIS.Application.Abstractions.AdminAbstractions
{
    public interface ILogService
    {
        Task<IEnumerable<UserOperationResponse>> GetLogsAsync(int userId);
        Task<IEnumerable<UserOperationResponse>> GetAllLogsAsync();
    }
}
