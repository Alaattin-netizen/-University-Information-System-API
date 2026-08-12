using System;

namespace UIS.Application.DTOs.Admin.Semester;

public class UpdateSemesterRequest
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime RegistrationStart { get; set; }
    public DateTime RegistrationEnd { get; set; }
    public bool IsActive { get; set; }
}