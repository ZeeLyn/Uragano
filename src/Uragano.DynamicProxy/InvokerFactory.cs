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

            var circuitBreakerAttr = clientMethodInfo.GetCustomAttribute<CircuitBreakerAttribute>();

            ServiceInvokers.TryAdd(route, new ServiceDescriptor
            {
                Route = route,
                MethodInfo = serverMethodInfo,
                MethodInvoker = new MethodInvoker(serverMethodInfo),
                ServerInterceptors = serverInterceptors,
                ClientInterceptors = clientInterceptors
            });
        }

        public ServiceDescriptor Get(string route)
        {
            if (ServiceInvokers.TryGetValue(route.ToLower(), out var value))
                return value;
            throw new NotFoundRouteException(route);
        }


        public async Task<object> Invoke(string route, object[] args, Dictionary<string, string> meta)
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
