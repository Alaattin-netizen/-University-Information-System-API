using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UIS.Application.Abstractions.StudentAbstractions;

public interface IModule
{
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
}