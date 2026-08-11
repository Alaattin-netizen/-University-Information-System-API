namespace UIS.Application.DTOs.Admin;

public class CreateStudentRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int? DepartmentId { get; set; }
    public int? AdvisorId { get; set; }
}