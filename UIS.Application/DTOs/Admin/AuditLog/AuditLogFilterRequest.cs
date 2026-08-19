namespace UIS.Application.DTOs.Filters;

public class AuditLogFilterRequest
{
    public int? UserId { get; set; }
    public string? Action { get; set; }
    public string? EntityType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}