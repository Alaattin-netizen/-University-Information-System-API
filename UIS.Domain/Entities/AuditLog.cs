using System.ComponentModel.DataAnnotations;

namespace UIS.Domain.Entities;

public class AuditLog 
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }

    [MaxLength(256)]
    public string UserEmail { get; set; }

    [MaxLength(50)]
    public string UserRole { get; set; }

    [MaxLength(50)]
    public string Action { get; set; }

    [MaxLength(50)]
    public string EntityType { get; set; }

    public int? EntityId { get; set; }

    public string? Details { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(45)]
    public string? IpAddress { get; set; }
}