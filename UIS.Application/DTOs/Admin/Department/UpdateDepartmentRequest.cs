namespace UIS.Application.DTOs.Admin.Department;

public class UpdateDepartmentRequest
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int FacultyId { get; set; }
}