using UIS.Domain.Entities.Users;

namespace UIS.Domain.Entities;

public class Instructor : User
{
    public int? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }

    // An instructor teaches many course offerings
    public virtual ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();

    // An instructor advises many students
    public virtual ICollection<Student> Advisees { get; set; } = new List<Student>();
}