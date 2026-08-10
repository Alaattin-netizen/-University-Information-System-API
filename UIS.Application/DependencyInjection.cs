using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UIS.Infrastructure.Data;
using UIS.Infrastructure.Repositories;

// ✅ Student Abstractions (for student-related services)
using UIS.Application.Abstractions.StudentAbstractions;

// ✅ Instructor Abstractions (for instructor-related services)
using UIS.Application.Abstractions.InstructorAbstractions;

// ✅ Student Services
using UIS.Application.Services.StudentServices;

// ✅ Instructor Services
using UIS.Application.Services.InstructorServices;

// ✅ General Services
using UIS.Application.Services;
using UIS.Application.Abstractions;

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

        return services;
    }
}