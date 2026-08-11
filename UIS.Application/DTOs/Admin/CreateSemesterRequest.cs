namespace UIS.Application.DTOs.Admin;

public class CreateSemesterRequest
{
    public string Name { get; set; } 
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime RegistrationStart { get; set; }
    public DateTime RegistrationEnd { get; set; }
    public bool IsActive { get; set; }
}