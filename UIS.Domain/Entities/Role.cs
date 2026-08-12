using System.ComponentModel.DataAnnotations;

namespace UIS.Domain.Entities;

public class Role 
{
    [Key] public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    // Navigation: Many-to-Many with Users
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}