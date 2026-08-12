namespace UIS.Application.DTOs.Admin;

public class UpdateFacultyRequest
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? DeanName { get; set; }
}