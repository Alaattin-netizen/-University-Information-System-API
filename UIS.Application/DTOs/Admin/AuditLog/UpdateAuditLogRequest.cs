namespace UIS.Application.DTOs.Admin.AuditLog;

public class UpdateAuditLogRequest
{
    public int Id { get; set; }
    public string? Details { get; set; } // Only details can be updated, maybe
}