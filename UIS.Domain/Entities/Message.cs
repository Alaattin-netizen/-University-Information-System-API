using System.ComponentModel.DataAnnotations;

namespace UIS.Domain.Entities;

public class Message 
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(500)]
    public string Subject { get; set; }

    [Required]
    public string Content { get; set; }

    public DateTime SentDate { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; } = false;

    public DateTime? ReadDate { get; set; } 
    public int SenderStudentId { get; set; }
    public virtual User Sender { get; set; }

    public int ReceiverInstructorId { get; set; }
    public virtual User Receiver { get; set; }
}