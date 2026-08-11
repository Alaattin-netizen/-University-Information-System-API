using System.ComponentModel.DataAnnotations;
using UIS.Domain.Enums;
namespace UIS.Domain.Entities.Users;

public abstract class User 
{
    [Key] 
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; }

    [Required, MaxLength(100)]
    public string LastName { get; set; }

    [Required, MaxLength(256)]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; } 

    [Required]
    public String Role { get; set; } 
}


