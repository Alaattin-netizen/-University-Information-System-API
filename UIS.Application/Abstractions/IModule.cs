using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UIS.Application.Abstractions;

public interface IModule
{
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
}