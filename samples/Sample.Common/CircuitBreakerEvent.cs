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
            Logger.LogTrace("Raise OnFallback");
        }

        public async Task OnBreak(string route, MethodInfo methodInfo, Exception exception, TimeSpan time)
        {
            Logger.LogTrace($"Raise OnBreak;{exception.Message}");
        }

        public async Task OnRest(string route, MethodInfo methodInfo)
        {
            Logger.LogTrace("Raise OnRest");
        }

        public async Task OnHalfOpen(string route, MethodInfo methodInfo)
        {
            Logger.LogTrace("Raise OnHalfOpen");
        }

        public async Task OnTimeOut(string route, MethodInfo methodInfo, Exception exception)
        {
            Logger.LogTrace($"Raise OnTimeOut;{exception.Message}");
        }

        public async Task OnRetry(string route, MethodInfo methodInfo, Exception exception, int retryTimes)
        {
            Logger.LogTrace($"Raise OnRetry;{exception.Message};{retryTimes}");
        }

        public async Task OnBulkheadRejected(string route, MethodInfo methodInfo)
        {
            Logger.LogTrace("Raise OnBulkheadRejected;");
        }
    }
}
