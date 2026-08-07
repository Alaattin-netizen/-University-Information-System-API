using System.ComponentModel.DataAnnotations;
using UIS.Domain.Entities.Users;

namespace UIS.Domain.Entities;

public class Department
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    public int? FacultyId { get; set; }
    public virtual Faculty? Faculty { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    public virtual ICollection<Instructor> Instructors { get; set; } = new List<Instructor>();
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}