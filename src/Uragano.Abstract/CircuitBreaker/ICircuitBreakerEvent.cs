using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Uragano.Abstractions.CircuitBreaker
{
    public interface ICircuitBreakerEvent
    {
        Task OnFallback(string route, MethodInfo methodInfo);

        Task OnBreak(string route, MethodInfo methodInfo, Exception exception, TimeSpan time);

        Task OnRest(string route, MethodInfo methodInfo);

        Task OnHalfOpen(string route, MethodInfo methodInfo);

        Task OnTimeOut(string route, MethodInfo methodInfo, Exception exception);

        Task OnRetry(string route, MethodInfo methodInfo, Exception exception, int retryTimes);
    }
}
