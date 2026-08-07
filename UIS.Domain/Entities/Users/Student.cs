namespace UIS.Domain.Entities.Users;

public class Student : User
{
    public int? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }

    public int? AdvisorId { get; set; }
    public virtual Instructor? Advisor { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}