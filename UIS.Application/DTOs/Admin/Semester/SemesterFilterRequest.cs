namespace UIS.Application.DTOs.Filters;

public class SemesterFilterRequest
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}