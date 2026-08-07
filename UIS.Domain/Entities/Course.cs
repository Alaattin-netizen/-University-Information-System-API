using System.ComponentModel.DataAnnotations;

namespace UIS.Domain.Entities;

public class Course 
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string Code { get; set; } // e.g., "MATH101"

    [Required, MaxLength(200)]
    public string Name { get; set; }

    public int Credits { get; set; }
    public int ECTS { get; set; }
    public int Quota { get; set; }
    public bool IsMandatory { get; set; }

    // Self-referencing Prerequisite
    public int? PrerequisiteCourseId { get; set; }
    public virtual Course? PrerequisiteCourse { get; set; }

    // Navigation
    public int? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }

    public virtual ICollection<CourseOffering> Offerings { get; set; } = new List<CourseOffering>();
}