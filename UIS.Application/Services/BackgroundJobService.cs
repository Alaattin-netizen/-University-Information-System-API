using System.Linq.Expressions;
using Hangfire;
using UIS.Application.Abstractions;

namespace UIS.Infrastructure.Services;

public class BackgroundJobService : IBackgroundJobService
{
    public void Enqueue<T>(Expression<Action<T>> methodCall)
    {
        BackgroundJob.Enqueue<T>(methodCall);
    }

    public void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
    {
        BackgroundJob.Schedule<T>(methodCall, delay);
    }
}