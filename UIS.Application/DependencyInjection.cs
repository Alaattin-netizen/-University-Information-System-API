using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UIS.Application.Abstractions;
using UIS.Application.Abstractions.AdminAbstractions;

// ✅ Instructor Abstractions (for instructor-related services)
using UIS.Application.Abstractions.InstructorAbstractions;
// ✅ Student Abstractions (for student-related services)
using UIS.Application.Abstractions.StudentAbstractions;
// ✅ General Services
using UIS.Application.Services;
using UIS.Application.Services.AdminServices;

// ✅ Instructor Services
using UIS.Application.Services.InstructorServices;
// ✅ Student Services
using UIS.Application.Services.StudentServices;
using UIS.Infrastructure.Data;
using UIS.Infrastructure.Repositories;

namespace UIS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

        // ---- General Services ----
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<LoggingHelper>();
        services.AddHttpContextAccessor();

        // ---- Student Services ----
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IGradeService, GradeService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IProfileService, ProfileService>();

        var test = typeof(DiagnosticStudentAbstraction);

        // ---- Instructor Services ----
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IStudentService, StudentService>();

        //---- Admin Services ----
        services.AddScoped<IFacultyService, FacultyService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISemesterService, SemesterService>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<ICourseOfferingService, CourseOfferingService>();
        services.AddScoped<IAdminEnrollmentService, AdminEnrollmentService>();
        services.AddScoped<IAdminMessageService, AdminMessageService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();
        return services;
    }
}