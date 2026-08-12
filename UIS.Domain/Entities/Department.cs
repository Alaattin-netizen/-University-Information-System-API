using System.ComponentModel.DataAnnotations;

namespace UIS.Domain.Entities;

public class Department 
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(100)]
    public string Name { get; set; }

    public int FacultyId { get; set; }
    public virtual Faculty Faculty { get; set; }

    // Navigation: both Students and Instructors are stored in Users table
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}