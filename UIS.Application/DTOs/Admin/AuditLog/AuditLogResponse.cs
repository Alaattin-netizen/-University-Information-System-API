namespace UIS.Application.DTOs.Admin.AuditLog;

public class AuditLogResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; }
    public string UserRole { get; set; }
    public string Action { get; set; }
    public string EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}