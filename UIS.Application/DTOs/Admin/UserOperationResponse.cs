namespace UIS.Application.DTOs.Admin;

public class UserOperationResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; }
    public string UserRole { get; set; }
    public string Action { get; set; } // e.g., "Created", "Updated", "Deleted", "LoggedIn"
    public string EntityType { get; set; } // e.g., "Student", "Course", "Enrollment"
    public int? EntityId { get; set; }
    public string Details { get; set; }
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; }
}