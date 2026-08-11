namespace UIS.Application.DTOs.Admin;

public class UpdateSemesterRequest
{
  
    public int SemesterId { get; set; }
    public DateTime RegistrationStart { get; set; }
    public DateTime RegistrationEnd { get; set; }

    public bool IsActive { get; set; }
}