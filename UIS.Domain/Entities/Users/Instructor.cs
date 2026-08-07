using UIS.Domain.Entities.Users;

namespace UIS.Domain.Entities;

public class Instructor : User
{
    public int? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }

    public virtual ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();

    public virtual ICollection<Student> Advisees { get; set; } = new List<Student>();
}