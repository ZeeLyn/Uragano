using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Uragano.Abstractions.Exceptions;
using Uragano.Abstractions.ServiceInvoker;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.CircuitBreaker;
using Uragano.Codec.MessagePack;
using Uragano.DynamicProxy.Interceptor;
using ServiceDescriptor = Uragano.Abstractions.ServiceDescriptor;

namespace Uragano.DynamicProxy
{
    public class InvokerFactory : IInvokerFactory
    {
        private static readonly ConcurrentDictionary<string, ServiceDescriptor>
            ServiceInvokers = new ConcurrentDictionary<string, ServiceDescriptor>();


        private IServiceProvider ServiceProvider { get; }

        private UraganoSettings UraganoSettings { get; }

        private IScriptInjection ScriptInjection { get; }

        public InvokerFactory(IServiceProvider serviceProvider, UraganoSettings uraganoSettings, IScriptInjection scriptInjection)
        {
            ServiceProvider = serviceProvider;
            UraganoSettings = uraganoSettings;
            ScriptInjection = scriptInjection;
        }

        public void Create(string route, MethodInfo serverMethodInfo, MethodInfo clientMethodInfo, List<Type> serverInterceptors, List<Type> clientInterceptors)
        {
            route = route.ToLower();
            if (ServiceInvokers.ContainsKey(route))
                throw new DuplicateRouteException(route);
            var enableClient = ServiceProvider.GetService<ILoadBalancing>() != null;
            var serviceDescriptor = new ServiceDescriptor
            {
                Route = route,
                MethodInfo = serverMethodInfo,
                ArgsType = serverMethodInfo == null ? null : serverMethodInfo.GetParameters().Select(p => p.ParameterType).ToArray(),
                MethodInvoker = serverMethodInfo == null ? null : new MethodInvoker(serverMethodInfo),
                ServerInterceptors = serverInterceptors,
                ClientInterceptors = clientInterceptors
            };
            var nonCircuitBreakerAttr = clientMethodInfo.GetCustomAttribute<NonCircuitBreakerAttribute>();
            if (nonCircuitBreakerAttr == null)
            {
                var circuitBreakerAttr = clientMethodInfo.GetCustomAttribute<CircuitBreakerAttribute>();
                var globalCircuitBreaker = UraganoSettings.CircuitBreakerOptions;
                if (globalCircuitBreaker != null || circuitBreakerAttr != null)
                {
                    var breaker = new ServiceCircuitBreakerOptions();
                    if (globalCircuitBreaker != null)
                    {
                        breaker.Timeout = globalCircuitBreaker.Timeout;
                        breaker.Retry = globalCircuitBreaker.Retry;
                        breaker.ExceptionsAllowedBeforeBreaking = globalCircuitBreaker.ExceptionsAllowedBeforeBreaking;
                        breaker.DurationOfBreak = globalCircuitBreaker.DurationOfBreak;
                    }

                    if (circuitBreakerAttr != null)
                    {
                        if (circuitBreakerAttr.Timeout.HasValue)
                            breaker.Timeout = circuitBreakerAttr.Timeout.Value;
                        if (circuitBreakerAttr.Retry.HasValue)
                            breaker.Retry = circuitBreakerAttr.Retry.Value;
                        if (circuitBreakerAttr.ExceptionsAllowedBeforeBreaking.HasValue)
                            breaker.ExceptionsAllowedBeforeBreaking =
                                circuitBreakerAttr.ExceptionsAllowedBeforeBreaking.Value;
                        if (circuitBreakerAttr.DurationOfBreak.HasValue)
                            breaker.DurationOfBreak = circuitBreakerAttr.DurationOfBreak.Value;
                        if (!string.IsNullOrWhiteSpace(circuitBreakerAttr.FallbackExecuteScript))
                        {
                            breaker.HasInjection = true;
                            if (enableClient)
                            {
                                ScriptInjection.AddScript(route, circuitBreakerAttr.FallbackExecuteScript,
                                    circuitBreakerAttr.ScriptUsingNameSpaces);
                            }
                        }
                    }

                    serviceDescriptor.ServiceCircuitBreakerOptions = breaker;
                }
            }

            ServiceInvokers.TryAdd(route, serviceDescriptor);
        }

        public ServiceDescriptor Get(string route)
        {
            if (ServiceInvokers.TryGetValue(route.ToLower(), out var value))
                return value;
            throw new NotFoundRouteException(route);
        }


        public async Task<ResultMessage> Invoke(string route, object[] args, Dictionary<string, string> meta)
        {
            if (!ServiceInvokers.TryGetValue(route, out var service))
                throw new NotFoundRouteException(route);
            using (var scope = ServiceProvider.CreateScope())
            {
                var context = new InterceptorContext
                {
                    ServiceRoute = service.Route,
                    ServiceProvider = scope.ServiceProvider,
                    Args = args,
                    Meta = meta,
                    MethodInfo = service.MethodInfo
                };
                context.Interceptors.Push(typeof(ServerDefaultInterceptor));
                foreach (var interceptor in service.ServerInterceptors)
                {
                    context.Interceptors.Push(interceptor);
                }
                if (UraganoSettings.ServerGlobalInterceptors.Any())
                {
                    foreach (var interceptor in UraganoSettings.ServerGlobalInterceptors)
                    {
                        context.Interceptors.Push(interceptor);
                    }
                }

                return await ((IInterceptor)scope.ServiceProvider.GetRequiredService(context.Interceptors.Pop()))
                      .Intercept(context);
            }
        }
    }
}
