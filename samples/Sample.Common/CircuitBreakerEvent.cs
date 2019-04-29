using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions.CircuitBreaker;

namespace Sample.Common
{
    public class CircuitBreakerEvent : ICircuitBreakerEvent
    {
        private ILogger Logger { get; }

        public CircuitBreakerEvent(ILogger<CircuitBreakerEvent> logger)
        {
            Logger = logger;
        }
        public async Task OnFallback(string route, MethodInfo methodInfo)
        {
            Logger.LogWarning("Raise OnFallback");
            await Task.CompletedTask;
        }

        public async Task OnBreak(string route, MethodInfo methodInfo, Exception exception, TimeSpan time)
        {
            Logger.LogError($"Raise OnBreak;{exception.Message}");
            await Task.CompletedTask;
        }

        public async Task OnRest(string route, MethodInfo methodInfo)
        {
            Logger.LogWarning("Raise OnRest");
            await Task.CompletedTask;
        }

        public async Task OnHalfOpen(string route, MethodInfo methodInfo)
        {
            Logger.LogWarning("Raise OnHalfOpen");
            await Task.CompletedTask;
        }

        public async Task OnTimeOut(string route, MethodInfo methodInfo, Exception exception)
        {
            Logger.LogWarning($"Raise OnTimeOut;{exception.Message}");
            await Task.CompletedTask;
        }

        public async Task OnRetry(string route, MethodInfo methodInfo, Exception exception, int retryTimes)
        {
            Logger.LogWarning($"Raise OnRetry;{exception.Message};{retryTimes}");
            await Task.CompletedTask;
        }

        public async Task OnBulkheadRejected(string route, MethodInfo methodInfo)
        {
            Logger.LogWarning("Raise OnBulkheadRejected;");
            await Task.CompletedTask;
        }
    }
}
