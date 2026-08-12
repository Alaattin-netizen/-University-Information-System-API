namespace UIS.Application.DTOs.Admin.Faculty;

public class UpdateFacultyRequest
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? DeanName { get; set; }
}