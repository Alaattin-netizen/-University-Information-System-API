using System.ComponentModel.DataAnnotations;

namespace UIS.Domain.Entities;

public class Semester 
{
    [Key] public int Id { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } // "Fall 2026"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime RegistrationStart { get; set; }
    public DateTime RegistrationEnd { get; set; }
    public bool IsActive { get; set; }

    public virtual ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
}