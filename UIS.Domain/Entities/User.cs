using System.ComponentModel.DataAnnotations;

namespace UIS.Domain.Entities;

public class User 
{
    [Key] public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; }

    [Required, MaxLength(100)]
    public string LastName { get; set; }

    [Required, MaxLength(256)]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    // Foreign Keys
    public int? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }

    public int? AdvisorId { get; set; } // Self-reference: Student -> Instructor
    public virtual User? Advisor { get; set; }

    // Navigation: Many-to-Many with Roles
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}