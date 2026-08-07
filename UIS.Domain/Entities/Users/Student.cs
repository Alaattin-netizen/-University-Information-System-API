namespace UIS.Domain.Entities;

public class Student : User
{
    // Foreign Key to Department
    public int? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }

    // Foreign Key to Advisor (Instructor)
    public int? AdvisorId { get; set; }
    public virtual Instructor? Advisor { get; set; }

    // Navigation property: A student has many enrollments
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}