using Microsoft.EntityFrameworkCore;
using System.Data;
using UIS.Domain.Entities;
using UIS.Domain.Entities.Users;
using UIS.Domain.Enums; 
namespace UIS.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for all your entities
    public DbSet<User> Users { get; set; }          // Base class - EF handles the TPH inheritance
    public DbSet<Student> Students { get; set; }
    public DbSet<Instructor> Instructors { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Faculty> Faculties { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<CourseOffering> CourseOfferings { get; set; }
    public DbSet<Semester> Semesters { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
   
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<Announcement> Announcements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- 1. Configure Inheritance (TPH) ---
        modelBuilder.Entity<User>()
            .HasDiscriminator(u => u.Role)
            .HasValue<Student>(Role.Student)
            .HasValue<Instructor>(Role.Instructor)
            .HasValue<Admin>(Role.Admin);

        // --- 2. Unique Constraints ---
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Ensure no duplicate Course Codes.
        modelBuilder.Entity<Course>()
            .HasIndex(c => c.Code)
            .IsUnique();

        // --- 3. Relationships & Cascade Delete ---

        // User (Student) -> Advisor (Instructor) - No cascade delete to avoid cycles.
        modelBuilder.Entity<Student>()
            .HasOne(s => s.Advisor)
            .WithMany(a => a.Advisees)
            .HasForeignKey(s => s.AdvisorId)
            .OnDelete(DeleteBehavior.SetNull); // If Instructor is deleted, set AdvisorId to NULL.

        // Course -> PrerequisiteCourse (Self-referencing)
        modelBuilder.Entity<Course>()
            .HasOne(c => c.PrerequisiteCourse)
            .WithMany()
            .HasForeignKey(c => c.PrerequisiteCourseId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a course if it's used as a prereq.

        // CourseOffering -> Course
        modelBuilder.Entity<CourseOffering>()
            .HasOne(co => co.Course)
            .WithMany(c => c.Offerings)
            .HasForeignKey(co => co.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // CourseOffering -> Instructor
        modelBuilder.Entity<CourseOffering>()
            .HasOne(co => co.Instructor)
            .WithMany(i => i.CourseOfferings)
            .HasForeignKey(co => co.InstructorId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting an instructor with active offerings.

        // Enrollment -> Student
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Enrollment -> CourseOffering
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.CourseOffering)
            .WithMany(co => co.Enrollments)
            .HasForeignKey(e => e.CourseOfferingId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasIndex(e => new { e.StudentId, e.CourseOfferingId })
                  .IsUnique()
                  .HasDatabaseName("IX_Enrollment_Student_Course"); // Optional: nice naming

            
            entity.Property(e => e.LetterGrade)
                  .HasMaxLength(2);

            // 3. Configure the relationship with Student (One Student -> Many Enrollments)
            entity.HasOne(e => e.Student)
                  .WithMany(s => s.Enrollments)
                  .HasForeignKey(e => e.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CourseOffering)
                  .WithMany(co => co.Enrollments)
                  .HasForeignKey(e => e.CourseOfferingId)
                  .OnDelete(DeleteBehavior.Cascade);   });

        modelBuilder.Entity<Announcement>()
            .HasOne(a => a.CourseOffering)
            .WithMany(co => co.Announcements)
            .HasForeignKey(a => a.CourseOfferingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(s => s.Messages)
            .HasForeignKey(m => m.SenderStudentId)
            .OnDelete(DeleteBehavior.Restrict); 

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverInstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        
    }
}