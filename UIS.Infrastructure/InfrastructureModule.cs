using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UIS.Application.Abstractions;
using UIS.Infrastructure.Data;

namespace UIS.Infrastructure;

public class InfrastructureModule : IModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register Repositories, Unit of Work, etc.
        // services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}