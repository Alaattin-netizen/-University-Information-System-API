using System.ComponentModel.DataAnnotations;

namespace UIS.Domain.Entities;

public class Faculty 
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(50)]
    public string? DeanName { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
}