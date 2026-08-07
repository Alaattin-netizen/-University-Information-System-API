using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UIS.Application.Abstractions;
using UIS.Application.Services;
using UIS.Infrastructure.Data;
using UIS.Infrastructure.Repositories;

namespace UIS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // ✅ Register Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

        // ✅ Register Services
        services.AddScoped<IJwtService, JwtService>(); 
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}