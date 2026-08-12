using Microsoft.EntityFrameworkCore;
using UIS.Domain.Entities;

namespace UIS.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Faculty> Faculties { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Semester> Semesters { get; set; }
    public DbSet<CourseOffering> CourseOfferings { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Role>().HasData(
    new Role { Id = 1, Name = "Admin", Description = "Full system access" },
    new Role { Id = 2, Name = "Instructor", Description = "Teaches courses" },
    new Role { Id = 3, Name = "Student", Description = "Enrolls in courses" }
);

        // --- Users ---
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // --- UserRole (Many-to-Many) ---
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => ur.Id);

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(ur => new { ur.UserId, ur.RoleId })
                .IsUnique();
        });

        // --- Roles ---
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        // --- User -> Advisor (Self-reference) ---
        modelBuilder.Entity<User>()
            .HasOne(u => u.Advisor)
            .WithMany() // No navigation property on the other side
            .HasForeignKey(u => u.AdvisorId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- User -> Department ---
        modelBuilder.Entity<User>()
            .HasOne(u => u.Department)
            .WithMany(d => d.Users) // ✅ Department has Users collection
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Department -> Faculty ---
        modelBuilder.Entity<Department>()
            .HasOne(d => d.Faculty)
            .WithMany(f => f.Departments)
            .HasForeignKey(d => d.FacultyId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Course -> Department ---
        modelBuilder.Entity<Course>()
            .HasOne(c => c.Department)
            .WithMany(d => d.Courses)
            .HasForeignKey(c => c.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Course -> Prerequisite (Self-reference) ---
        modelBuilder.Entity<Course>()
            .HasOne(c => c.PrerequisiteCourse)
            .WithMany()
            .HasForeignKey(c => c.PrerequisiteCourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Course -> Code unique ---
        modelBuilder.Entity<Course>()
            .HasIndex(c => c.Code)
            .IsUnique();

        // --- CourseOffering -> Course ---
        modelBuilder.Entity<CourseOffering>()
            .HasOne(co => co.Course)
            .WithMany(c => c.Offerings)
            .HasForeignKey(co => co.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- CourseOffering -> Instructor (User) ---
        modelBuilder.Entity<CourseOffering>()
            .HasOne(co => co.Instructor)
            .WithMany() // Instructors don't need a collection of offerings (optional)
            .HasForeignKey(co => co.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- CourseOffering -> Semester ---
        modelBuilder.Entity<CourseOffering>()
            .HasOne(co => co.Semester)
            .WithMany(s => s.CourseOfferings)
            .HasForeignKey(co => co.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Enrollment ---
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasIndex(e => new { e.StudentId, e.CourseOfferingId })
                .IsUnique();

            entity.HasOne(e => e.Student)
                .WithMany() // Students don't need a collection of enrollments (optional)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CourseOffering)
                .WithMany(co => co.Enrollments)
                .HasForeignKey(e => e.CourseOfferingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Attendance ---
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.CourseOffering)
                .WithMany(co => co.Attendances)
                .HasForeignKey(a => a.CourseOfferingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Announcement ---
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasOne(a => a.CourseOffering)
                .WithMany(co => co.Announcements)
                .HasForeignKey(a => a.CourseOfferingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Instructor)
                .WithMany()
                .HasForeignKey(a => a.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Message ---
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderStudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverInstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- AuditLog ---
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasOne(al => al.User)
                .WithMany()
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}