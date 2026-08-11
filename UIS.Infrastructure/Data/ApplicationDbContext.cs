using Microsoft.EntityFrameworkCore;
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
    public DbSet<User> Users { get; set; }
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
    public DbSet<Message> Messages { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- 1. Configure Inheritance (TPH) ---
        modelBuilder.Entity<User>()
            .HasDiscriminator(u => u.Role)
            .HasValue<Student>("Student")
            .HasValue<Instructor>("Instructor")
            .HasValue<Admin>("Admin");

        // --- 2. Unique Constraints ---
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Course>()
            .HasIndex(c => c.Code)
            .IsUnique();

        // --- 3. Configure Relationships ---

        // Student -> Department (Restrict to avoid cascade paths)
        modelBuilder.Entity<Student>()
            .HasOne(s => s.Department)
            .WithMany(d => d.Students)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Instructor -> Department (Restrict to avoid cascade paths)
        modelBuilder.Entity<Instructor>()
            .HasOne(i => i.Department)
            .WithMany(d => d.Instructors)
            .HasForeignKey(i => i.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Student -> Advisor (Instructor)
        modelBuilder.Entity<Student>()
            .HasOne(s => s.Advisor)
            .WithMany(a => a.Advisees)
            .HasForeignKey(s => s.AdvisorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Course -> PrerequisiteCourse (Self-referencing)
        modelBuilder.Entity<Course>()
            .HasOne(c => c.PrerequisiteCourse)
            .WithMany()
            .HasForeignKey(c => c.PrerequisiteCourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // CourseOffering -> Course
        modelBuilder.Entity<CourseOffering>()
            .HasOne(co => co.Course)
            .WithMany(c => c.Offerings)
            .HasForeignKey(co => co.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // CourseOffering -> Instructor
        modelBuilder.Entity<CourseOffering>()
            .HasOne(co => co.Instructor)
            .WithMany(i => i.CourseOfferings)
            .HasForeignKey(co => co.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        // CourseOffering -> Semester
        modelBuilder.Entity<CourseOffering>()
            .HasOne(co => co.Semester)
            .WithMany(s => s.CourseOfferings)
            .HasForeignKey(co => co.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- 4. Enrollment Configuration ---
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasIndex(e => new { e.StudentId, e.CourseOfferingId })
                  .IsUnique()
                  .HasDatabaseName("IX_Enrollment_Student_Course");

            entity.Property(e => e.LetterGrade).HasMaxLength(2);

            entity.HasOne(e => e.Student)
                  .WithMany(s => s.Enrollments)
                  .HasForeignKey(e => e.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CourseOffering)
                  .WithMany(co => co.Enrollments)
                  .HasForeignKey(e => e.CourseOfferingId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- 5. Attendance Configuration ---
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasOne(a => a.Student)
                  .WithMany() // No navigation property on Student for Attendance
                  .HasForeignKey(a => a.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.CourseOffering)
                  .WithMany(co => co.Attendances)
                  .HasForeignKey(a => a.CourseOfferingId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- 6. Announcement ---
        modelBuilder.Entity<Announcement>()
            .HasOne(a => a.CourseOffering)
            .WithMany(co => co.Announcements)
            .HasForeignKey(a => a.CourseOfferingId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- 7. Message ---
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(s => s.Messages)
            .HasForeignKey(m => m.SenderStudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany() // No navigation property on Instructor for received messages
            .HasForeignKey(m => m.ReceiverInstructorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}