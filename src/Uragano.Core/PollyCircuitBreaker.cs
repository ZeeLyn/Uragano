using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Timeout;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Uragano.Abstractions.CircuitBreaker;
using Uragano.Abstractions.Service;

namespace Uragano.Core
{
    public class PollyCircuitBreaker : ICircuitBreaker
    {
        private IServiceProvider ServiceProvider { get; }

        private IScriptInjection ScriptInjection { get; }

        private IServiceFactory ServiceFactory { get; }

        private static readonly ConcurrentDictionary<string, AsyncPolicy<object>> Policies = new ConcurrentDictionary<string, AsyncPolicy<object>>();

        public PollyCircuitBreaker(IServiceProvider serviceProvider, IScriptInjection scriptInjection, IServiceFactory serviceFactory)
        {
            ServiceProvider = serviceProvider;
            ScriptInjection = scriptInjection;
            ServiceFactory = serviceFactory;
        }

        public async Task<object> ExecuteAsync(string route, Func<Task<object>> action, Type returnValueType)
        {
            var policy = GetPolicy(route, returnValueType);
            return await policy.ExecuteAsync(action);
        }

        public async Task ExecuteAsync(string route, Func<Task> action)
        {
            var policy = GetPolicy(route, null);
            await policy.ExecuteAsync(async () =>
            {
                await action();
                return null;
            });
        }

        private AsyncPolicy<object> GetPolicy(string route, Type returnValueType)
        {
            return Policies.GetOrAdd(route, (key) =>
            {
                var service = ServiceFactory.Get(route);
                var serviceCircuitBreakerOptions = service.ServiceCircuitBreakerOptions;
                var circuitBreakerEvent = ServiceProvider.GetService<ICircuitBreakerEvent>();
                AsyncPolicy<object> policy = Policy<object>.Handle<Exception>().FallbackAsync(
                     async ct =>
                     {
                         if (circuitBreakerEvent != null)
                             await circuitBreakerEvent.OnFallback(route, service.ClientMethodInfo);
                         if (returnValueType == null)
                             return null;
                         if (service.ServiceCircuitBreakerOptions.HasInjection)
                             return await ScriptInjection.Run(route);
                         return returnValueType.IsValueType ? Activator.CreateInstance(returnValueType) : null;
                     });
                if (serviceCircuitBreakerOptions.ExceptionsAllowedBeforeBreaking > 0)
                {
                    policy = policy.WrapAsync(Policy.Handle<Exception>().CircuitBreakerAsync(serviceCircuitBreakerOptions.ExceptionsAllowedBeforeBreaking, serviceCircuitBreakerOptions.DurationOfBreak,
                        async (ex, state, ts, ctx) =>
                        {
                            if (circuitBreakerEvent != null)
                                await circuitBreakerEvent.OnBreak(route, service.ClientMethodInfo, ex, ts);
                        },
                        async ctx =>
                        {
                            if (circuitBreakerEvent != null)
                                await circuitBreakerEvent.OnRest(route, service.ClientMethodInfo);
                        },
                        async () =>
                        {
                            if (circuitBreakerEvent != null)
                                await circuitBreakerEvent.OnHalfOpen(route, service.ClientMethodInfo);
                        }));
                }
                if (serviceCircuitBreakerOptions.Retry > 0)
                {
                    policy = policy.WrapAsync(Policy.Handle<Exception>().RetryAsync(serviceCircuitBreakerOptions.Retry,
                        async (ex, times) =>
                        {
                            if (circuitBreakerEvent != null)
                                await circuitBreakerEvent.OnRetry(route, service.ClientMethodInfo, ex, times);
                        }));
                }

                if (serviceCircuitBreakerOptions.Timeout.Ticks > 0)
                {
                    policy = policy.WrapAsync(Policy.TimeoutAsync(serviceCircuitBreakerOptions.Timeout, TimeoutStrategy.Pessimistic,
                        async (ctx, ts, task, ex) =>
                        {
                            if (circuitBreakerEvent != null)
                                await circuitBreakerEvent.OnTimeOut(route, service.ClientMethodInfo, ex);
                        }));
                }
                return policy;
            });
        }

        //private AsyncPolicy GetPolicy(string route)
        //{
        //    return NoReturnValuePolicies.GetOrAdd(route, (key) =>
        //    {
        //        var service = InvokerFactory.Get(route);
        //        var serviceCircuitBreakerOptions = service.ServiceCircuitBreakerOptions;
        //        var circuitBreakerEvent = ServiceProvider.GetService<ICircuitBreakerEvent>();
        //        AsyncPolicy policy = Policy.Handle<Exception>().Or<BrokenCircuitException>().FallbackAsync(
        //             async ct =>
        //             {
        //                 if (circuitBreakerEvent != null)
        //                     await circuitBreakerEvent.OnFallback(route, service.MethodInfo);
        //                 if (service.ServiceCircuitBreakerOptions.HasInjection)
        //                     await ScriptInjection.Run(route);
        //             });



        //        if (serviceCircuitBreakerOptions.Timeout.Ticks > 0)
        //        {
        //            policy = policy.WrapAsync(Policy.TimeoutAsync(serviceCircuitBreakerOptions.Timeout, TimeoutStrategy.Pessimistic,
        //                async (ctx, ts, task, ex) =>
        //                {
        //                    if (circuitBreakerEvent != null)
        //                        await circuitBreakerEvent.OnTimeOut(route, service.MethodInfo, ex);
        //                }));

        //        }

        //        if (serviceCircuitBreakerOptions.Retry > 0)
        //        {
        //            policy = policy.WrapAsync(Policy.Handle<Exception>().RetryAsync(serviceCircuitBreakerOptions.Retry,
        //                async (ex, times) =>
        //                {
        //                    if (circuitBreakerEvent != null)
        //                        await circuitBreakerEvent.OnRetry(route, service.MethodInfo, ex, times);
        //                }));
        //        }

        //        if (serviceCircuitBreakerOptions.ExceptionsAllowedBeforeBreaking > 0)
        //        {
        //            policy = policy.WrapAsync(Policy.Handle<Exception>().CircuitBreakerAsync(serviceCircuitBreakerOptions.ExceptionsAllowedBeforeBreaking, serviceCircuitBreakerOptions.DurationOfBreak,
        //                async (ex, state, ts, ctx) =>
        //                {
        //                    if (circuitBreakerEvent != null)
        //                        await circuitBreakerEvent.OnBreak(route, service.MethodInfo, ex, ts);
        //                },
        //                async ctx =>
        //                {
        //                    if (circuitBreakerEvent != null)
        //                        await circuitBreakerEvent.OnRest(route, service.MethodInfo);
        //                },
        //                async () =>
        //                {
        //                    if (circuitBreakerEvent != null)
        //                        await circuitBreakerEvent.OnHalfOpen(route, service.MethodInfo);
        //                }));
        //        }
        //        return policy;
        //    });
        //}
    }
}
