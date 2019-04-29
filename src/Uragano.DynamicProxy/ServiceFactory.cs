using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Uragano.Abstractions.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.CircuitBreaker;
using Uragano.Abstractions.Service;
using Uragano.DynamicProxy.Interceptor;
using ServiceDescriptor = Uragano.Abstractions.ServiceDescriptor;

namespace Uragano.DynamicProxy
{
    public class ServiceFactory : IServiceFactory
    {
        private static readonly ConcurrentDictionary<string, ServiceDescriptor>
            ServiceInvokers = new ConcurrentDictionary<string, ServiceDescriptor>(StringComparer.OrdinalIgnoreCase);


        private IServiceProvider ServiceProvider { get; }

        private UraganoSettings UraganoSettings { get; }

        private IScriptInjection ScriptInjection { get; }


        public ServiceFactory(IServiceProvider serviceProvider, UraganoSettings uraganoSettings, IScriptInjection scriptInjection)
        {
            ServiceProvider = serviceProvider;
            UraganoSettings = uraganoSettings;
            ScriptInjection = scriptInjection;
        }

        public void Create(string route, MethodInfo serverMethodInfo, MethodInfo clientMethodInfo, List<Type> serverInterceptors, List<Type> clientInterceptors)
        {
            if (ServiceInvokers.ContainsKey(route))
                throw new DuplicateRouteException(route);
            var enableClient = ServiceProvider.GetService<ILoadBalancing>() != null;
            ServiceCircuitBreakerOptions breaker = null;
            CachingConfig cachingConfig = null;
            if (enableClient)
            {
                #region Circuit breaker
                var nonCircuitBreakerAttr = clientMethodInfo.GetCustomAttribute<NonCircuitBreakerAttribute>();
                if (nonCircuitBreakerAttr == null)
                {
                    var circuitBreakerAttr = clientMethodInfo.GetCustomAttribute<CircuitBreakerAttribute>();
                    var globalCircuitBreaker = UraganoSettings.CircuitBreakerOptions;
                    if (globalCircuitBreaker != null || circuitBreakerAttr != null)
                    {
                        breaker = new ServiceCircuitBreakerOptions();
                        if (globalCircuitBreaker != null)
                        {
                            breaker.Timeout = globalCircuitBreaker.Timeout;
                            breaker.Retry = globalCircuitBreaker.Retry;
                            breaker.ExceptionsAllowedBeforeBreaking =
                                globalCircuitBreaker.ExceptionsAllowedBeforeBreaking;
                            breaker.DurationOfBreak = globalCircuitBreaker.DurationOfBreak;
                            breaker.MaxParallelization = globalCircuitBreaker.MaxParallelization;
                            breaker.MaxQueuingActions = globalCircuitBreaker.MaxQueuingActions;
                        }

                        if (circuitBreakerAttr != null)
                        {
                            if (circuitBreakerAttr.TimeoutMilliseconds > -1)
                                breaker.Timeout = TimeSpan.FromMilliseconds(circuitBreakerAttr.TimeoutMilliseconds);
                            if (circuitBreakerAttr.Retry > -1)
                                breaker.Retry = circuitBreakerAttr.Retry;
                            if (circuitBreakerAttr.ExceptionsAllowedBeforeBreaking > -1)
                                breaker.ExceptionsAllowedBeforeBreaking =
                                    circuitBreakerAttr.ExceptionsAllowedBeforeBreaking;
                            if (circuitBreakerAttr.DurationOfBreakSeconds > -1)
                                breaker.DurationOfBreak = TimeSpan.FromSeconds(circuitBreakerAttr.DurationOfBreakSeconds);
                            if (!string.IsNullOrWhiteSpace(circuitBreakerAttr.FallbackExecuteScript))
                            {
                                breaker.HasInjection = true;
                                ScriptInjection.AddScript(route, circuitBreakerAttr.FallbackExecuteScript,
                                    circuitBreakerAttr.ScriptUsingNameSpaces);
                            }

                            if (circuitBreakerAttr.MaxParallelization > -1)
                                breaker.MaxParallelization = circuitBreakerAttr.MaxParallelization;

                            if (circuitBreakerAttr.MaxQueuingActions > -1)
                                breaker.MaxQueuingActions = circuitBreakerAttr.MaxQueuingActions;
                        }
                    }
                }
                #endregion

                #region Caching
                //Must have a method of returning a value.
                if (UraganoSettings.CachingOptions != null && clientMethodInfo.ReturnType != typeof(Task) && clientMethodInfo.GetCustomAttribute<NonCachingAttribute>() == null && clientMethodInfo.DeclaringType?.GetCustomAttribute<NonCachingAttribute>() == null)
                {
                    var attr = clientMethodInfo.GetCustomAttribute<CachingAttribute>();
                    var keyGenerator = ServiceProvider.GetRequiredService<ICachingKeyGenerator>();
                    var key = keyGenerator.GenerateKeyPlaceholder(UraganoSettings.CachingOptions.KeyPrefix, UraganoSettings.CachingOptions.ExpireSeconds, route, clientMethodInfo, attr);

                    cachingConfig = new CachingConfig(key, attr != null && !string.IsNullOrWhiteSpace(attr.Key), attr != null && attr.ExpireSeconds != -1 ? attr.ExpireSeconds : UraganoSettings.CachingOptions.ExpireSeconds);
                }
                #endregion
            }
            var serviceDescriptor = new ServiceDescriptor(route, serverMethodInfo, clientMethodInfo, serverMethodInfo == null ? null : new MethodInvoker(serverMethodInfo), serverInterceptors, clientInterceptors, breaker, cachingConfig);
            ServiceInvokers.TryAdd(route, serviceDescriptor);
        }

        public ServiceDescriptor Get(string route)
        {
            if (ServiceInvokers.TryGetValue(route, out var value))
                return value;
            throw new NotFoundRouteException(route);
        }


        public async Task<IServiceResult> InvokeAsync(string route, object[] args, Dictionary<string, string> meta)
        {
            var service = Get(route);
            using (var scope = ServiceProvider.CreateScope())
            {
                var context = new InterceptorContext
                {
                    ServiceRoute = service.Route,
                    ServiceProvider = scope.ServiceProvider,
                    Args = args,
                    Meta = meta,
                    MethodInfo = service.ServerMethodInfo
                };
                context.Interceptors.Push(typeof(ServerDefaultInterceptor));
                foreach (var interceptor in service.ServerInterceptors)
                {
                    context.Interceptors.Push(interceptor);
                }

                return await ((IInterceptor)scope.ServiceProvider.GetRequiredService(context.Interceptors.Pop()))
                      .Intercept(context);
            }
        }
    }
}
