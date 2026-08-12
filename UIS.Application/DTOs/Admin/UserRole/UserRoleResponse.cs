namespace UIS.Application.DTOs.Admin.UserRole;

public class UserRoleResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; }
    public string UserFullName { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; }
    public DateTime AssignedAt { get; set; }
}