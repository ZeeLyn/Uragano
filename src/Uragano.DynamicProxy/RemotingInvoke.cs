using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceInvoker;
using Uragano.DynamicProxy.Interceptor;
using Uragano.Remoting;

namespace Uragano.DynamicProxy
{
    public class RemotingInvoke : IRemotingInvoke
    {
        private ILoadBalancing LoadBalancing { get; }
        private IClientFactory ClientFactory { get; }
        private IInvokerFactory InvokerFactory { get; }
        private UraganoSettings UraganoSettings { get; }
        private IServiceProvider ServiceProvider { get; }

        public RemotingInvoke(ILoadBalancing loadBalancing, IClientFactory clientFactory, IInvokerFactory invokerFactory, UraganoSettings uraganoSettings, IServiceProvider serviceProvider)
        {
            LoadBalancing = loadBalancing;
            ClientFactory = clientFactory;
            InvokerFactory = invokerFactory;
            UraganoSettings = uraganoSettings;
            ServiceProvider = serviceProvider;
        }
        public async Task<T> InvokeAsync<T>(object[] args, string route, string serviceName, Dictionary<string, string> meta = default)
        {
            var service = InvokerFactory.Get(route);

            using (var scope = ServiceProvider.CreateScope())
            {
                var context = new InterceptorContext
                {
                    ServiceProvider = scope.ServiceProvider,
                    Args = args,
                    ServiceRoute = route,
                    Meta = meta,
                    MethodInfo = service.MethodInfo,
                    ReturnType = typeof(T),
                    ServiceName = serviceName
                };

                context.Interceptors.Push(typeof(ClientDefaultInterceptor));
                foreach (var interceptor in service.ClientInterceptors)
                {
                    context.Interceptors.Push(interceptor);
                }

                if (UraganoSettings.ClientGlobalInterceptors.Any())
                {
                    foreach (var interceptor in UraganoSettings.ClientGlobalInterceptors)
                    {
                        context.Interceptors.Push(interceptor);
                    }
                }

                return (T)await ((IInterceptor)scope.ServiceProvider.GetRequiredService(context.Interceptors.Pop())).Intercept(context);
            }
        }

        public async Task InvokeAsync(object[] args, string route, string serviceName, Dictionary<string, string> meta = default)
        {
            var service = InvokerFactory.Get(route);

            using (var scope = ServiceProvider.CreateScope())
            {
                var context = new InterceptorContext
                {
                    ServiceProvider = scope.ServiceProvider,
                    Args = args,
                    ServiceRoute = route,
                    Meta = meta,
                    MethodInfo = service.MethodInfo,
                    ServiceName = serviceName
                };

                context.Interceptors.Push(typeof(ClientDefaultInterceptor));
                foreach (var interceptor in service.ClientInterceptors)
                {
                    context.Interceptors.Push(interceptor);
                }

                if (UraganoSettings.ClientGlobalInterceptors.Any())
                {
                    foreach (var interceptor in UraganoSettings.ClientGlobalInterceptors)
                    {
                        context.Interceptors.Push(interceptor);
                    }
                }

                await ((IInterceptor)scope.ServiceProvider.GetRequiredService(context.Interceptors.Pop())).Intercept(context);
            }
        }
    }
}
