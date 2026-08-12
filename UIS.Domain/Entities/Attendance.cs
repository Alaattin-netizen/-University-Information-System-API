using System.ComponentModel.DataAnnotations;

namespace UIS.Domain.Entities;

public class Attendance 
{
    [Key]
    public int Id { get; set; }
    public int StudentId { get; set; }
    public virtual User Student { get; set; }

    public int CourseOfferingId { get; set; }
    public virtual CourseOffering CourseOffering { get; set; }

    public DateTime Date { get; set; }
    public bool IsPresent { get; set; }
}