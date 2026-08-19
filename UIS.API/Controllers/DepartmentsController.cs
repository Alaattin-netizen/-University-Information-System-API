using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UIS.Application.DTOs.Admin.Department;
using UIS.Application.DTOs.Filters;
using UIS.Application.Services;

namespace UIS.API.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentsController : BaseApiController
{
    private readonly IFacultyService _facultyService;
    private readonly LoggingHelper _loggingHelper;

    public DepartmentsController(IFacultyService facultyService, LoggingHelper loggingHelper)
    {
        _facultyService = facultyService;
        _loggingHelper = loggingHelper;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] DepartmentFilterRequest filter)
        => Ok(await _facultyService.GetAllDepartmentsAsync(filter));

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _facultyService.GetDepartmentByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request)
    {
        var result = await _facultyService.CreateDepartmentAsync(request);
        await _loggingHelper.LogOperationAsync("Created", "Department", result.Id, $"Name: {request.Name}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateDepartmentRequest request)
    {
        var result = await _facultyService.UpdateDepartmentAsync(request);
        await _loggingHelper.LogOperationAsync("Updated", "Department", result.Id, $"Name: {request.Name}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _facultyService.DeleteDepartmentAsync(id);
        await _loggingHelper.LogOperationAsync("Deleted", "Department", id, $"ID: {id}", GetCurrentUserId(), GetCurrentUserEmail(), GetCurrentUserRoles());
        return NoContent();
    }
}