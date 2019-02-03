using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Uragano.Abstractions.CircuitBreaker;
using Uragano.Abstractions.ServiceInvoker;

namespace Uragano.Core
{
    public class PollyCircuitBreaker : ICircuitBreaker
    {
        private IServiceProvider ServiceProvider { get; }

        private IScriptInjection ScriptInjection { get; }

        private IInvokerFactory InvokerFactory { get; }

        private static readonly ConcurrentDictionary<string, Policy<object>> HasReturnValuePolicies = new ConcurrentDictionary<string, Policy<object>>();

        private static readonly ConcurrentDictionary<string, Policy> NoReturnValuePolicies = new ConcurrentDictionary<string, Policy>();

        public PollyCircuitBreaker(IServiceProvider serviceProvider, IScriptInjection scriptInjection, IInvokerFactory invokerFactory)
        {
            ServiceProvider = serviceProvider;
            ScriptInjection = scriptInjection;
            InvokerFactory = invokerFactory;
        }

        public async Task<object> ExecuteAsync(string route, Func<Task<object>> action, Type returnValueType)
        {
            var policy = GetPolicy(route, returnValueType);
            return await policy.ExecuteAsync(action);
        }

        public async Task ExecuteAsync(string route, Func<Task> action)
        {
            var policy = GetPolicy(route);
            await policy.ExecuteAsync(action);
        }

        private Policy<object> GetPolicy(string route, Type returnValueType)
        {
            return HasReturnValuePolicies.GetOrAdd(route, (key) =>
            {
                var service = InvokerFactory.Get(route);
                var serviceCircuitBreakerOptions = service.ServiceCircuitBreakerOptions;
                var circuitBreakerEvent = ServiceProvider.GetService<ICircuitBreakerEvent>();
                Policy<object> policy = Policy<object>.Handle<Exception>().FallbackAsync(
                     async ct =>
                     {

                         if (circuitBreakerEvent != null)
                             await circuitBreakerEvent.OnFallback(route, service.MethodInfo);
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
                                await circuitBreakerEvent.OnBreak(route, service.MethodInfo, ex, ts);
                        },
                        async ctx =>
                        {
                            if (circuitBreakerEvent != null)
                                await circuitBreakerEvent.OnRest(route, service.MethodInfo);
                        },
                        async () =>
                        {
                            if (circuitBreakerEvent != null)
                                await circuitBreakerEvent.OnHalfOpen(route, service.MethodInfo);
                        }));
                }
                if (serviceCircuitBreakerOptions.Retry > 0)
                {
                    policy = policy.WrapAsync(Policy.Handle<Exception>().RetryAsync(serviceCircuitBreakerOptions.Retry,
                        async (ex, times) =>
                        {
                            if (circuitBreakerEvent != null)
                                await circuitBreakerEvent.OnRetry(route, service.MethodInfo, ex, times);
                        }));
                }

                if (serviceCircuitBreakerOptions.Timeout.Ticks > 0)
                {
                    policy = policy.WrapAsync(Policy.TimeoutAsync(serviceCircuitBreakerOptions.Timeout, TimeoutStrategy.Pessimistic,
                        async (ctx, ts, task, ex) =>
                        {
                            if (circuitBreakerEvent != null)
                                await circuitBreakerEvent.OnTimeOut(route, service.MethodInfo, ex);
                        }));

                }
                return policy;
            });
        }

        private Policy GetPolicy(string route)
        {
            return NoReturnValuePolicies.GetOrAdd(route, (key) =>
            {
                var service = InvokerFactory.Get(route);
                var serviceCircuitBreakerOptions = service.ServiceCircuitBreakerOptions;
                var circuitBreakerEvent = ServiceProvider.GetService<ICircuitBreakerEvent>();
                Policy policy = Policy.Handle<Exception>().Or<BrokenCircuitException>().FallbackAsync(
                     async ct =>
                     {
                         if (circuitBreakerEvent != null)
                             await circuitBreakerEvent.OnFallback(route, service.MethodInfo);
                         if (service.ServiceCircuitBreakerOptions.HasInjection)
                             await ScriptInjection.Run(route);
                     });



                if (serviceCircuitBreakerOptions.Timeout.Ticks > 0)
                {
                    policy = policy.WrapAsync(Policy.TimeoutAsync(serviceCircuitBreakerOptions.Timeout, TimeoutStrategy.Pessimistic,
                        async (ctx, ts, task, ex) =>
                        {
                            if (circuitBreakerEvent != null)
                                await circuitBreakerEvent.OnTimeOut(route, service.MethodInfo, ex);
                        }));

                }

                if (serviceCircuitBreakerOptions.Retry > 0)
                {
                    policy = policy.WrapAsync(Policy.Handle<Exception>().RetryAsync(serviceCircuitBreakerOptions.Retry,
                        async (ex, times) =>
                        {
                            if (circuitBreakerEvent != null)
                                await circuitBreakerEvent.OnRetry(route, service.MethodInfo, ex, times);
                        }));
                }

                if (serviceCircuitBreakerOptions.ExceptionsAllowedBeforeBreaking > 0)
                {
                    policy = policy.WrapAsync(Policy.Handle<Exception>().CircuitBreakerAsync(serviceCircuitBreakerOptions.ExceptionsAllowedBeforeBreaking, serviceCircuitBreakerOptions.DurationOfBreak,
                        async (ex, state, ts, ctx) =>
                        {
                            if (circuitBreakerEvent != null)
                                await circuitBreakerEvent.OnBreak(route, service.MethodInfo, ex, ts);
                        },
                        async ctx =>
                        {
                            if (circuitBreakerEvent != null)
                                await circuitBreakerEvent.OnRest(route, service.MethodInfo);
                        },
                        async () =>
                        {
                            if (circuitBreakerEvent != null)
                                await circuitBreakerEvent.OnHalfOpen(route, service.MethodInfo);
                        }));
                }
                return policy;
            });
        }
    }
}
