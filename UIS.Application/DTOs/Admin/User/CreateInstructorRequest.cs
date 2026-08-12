namespace UIS.Application.DTOs.Admin.User;

public class CreateInstructorRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int DepartmentId { get; set; }
}