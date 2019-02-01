using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions.CircuitBreaker;

namespace Sample.Service.Interfaces
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
            Logger.LogDebug("Raise OnFallback");
        }

        public async Task OnBreak(string route, MethodInfo methodInfo, Exception exception, TimeSpan time)
        {
            Logger.LogDebug($"Raise OnBreak;{exception.Message}");
        }

        public async Task OnRest(string route, MethodInfo methodInfo)
        {
            Logger.LogDebug("Raise OnRest");
        }

        public async Task OnHalfOpen(string route, MethodInfo methodInfo)
        {
            Logger.LogDebug("Raise OnHalfOpen");
        }

        public async Task OnTimeOut(string route, MethodInfo methodInfo, Exception exception)
        {
            Logger.LogDebug($"Raise OnTimeOut;{exception.Message}");
        }

        public async Task OnRetry(string route, MethodInfo methodInfo, Exception exception, int retryTimes)
        {
            Logger.LogDebug($"Raise OnRetry;{exception.Message};{retryTimes}");
        }
    }
}
