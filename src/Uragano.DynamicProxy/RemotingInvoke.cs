using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.Exceptions;
using Uragano.Abstractions.Service;
using Uragano.DynamicProxy.Interceptor;

namespace Uragano.DynamicProxy
{
    public class RemotingInvoke : IRemotingInvoke
    {
        private IServiceFactory ServiceFactory { get; }
        private UraganoSettings UraganoSettings { get; }
        private IServiceProvider ServiceProvider { get; }

        public RemotingInvoke(IServiceFactory serviceFactory, UraganoSettings uraganoSettings, IServiceProvider serviceProvider)
        {
            ServiceFactory = serviceFactory;
            UraganoSettings = uraganoSettings;
            ServiceProvider = serviceProvider;
        }

        public async Task<T> InvokeAsync<T>(object[] args, string route, string serviceName, Dictionary<string, string> meta = default)
        {
            var service = ServiceFactory.Get(route);

            using (var scope = ServiceProvider.CreateScope())
            {
                var context = new InterceptorContext
                {
                    ServiceProvider = scope.ServiceProvider,
                    Args = args,
                    ServiceRoute = route,
                    Meta = meta,
                    MethodInfo = service.ClientMethodInfo,
                    ReturnType = typeof(T),
                    ServiceName = serviceName
                };

                context.Interceptors.Push(typeof(ClientDefaultInterceptor));
                if (service.CachingConfig != null)
                    context.Interceptors.Push(typeof(CachingDefaultInterceptor));
                foreach (var interceptor in service.ClientInterceptors)
                {
                    context.Interceptors.Push(interceptor);
                }

                var result = await ((IInterceptor)scope.ServiceProvider.GetRequiredService(context.Interceptors.Pop())).Intercept(context);
                if (result.Status != RemotingStatus.Ok)
                    throw new RemoteInvokeException(route, result.Result?.ToString(), result.Status);

                return result.Result == null ? default : (T)result.Result;
            }
        }

        public async Task InvokeAsync(object[] args, string route, string serviceName, Dictionary<string, string> meta = default)
        {
            var service = ServiceFactory.Get(route);

            using (var scope = ServiceProvider.CreateScope())
            {
                var context = new InterceptorContext
                {
                    ServiceProvider = scope.ServiceProvider,
                    Args = args,
                    ServiceRoute = route,
                    Meta = meta,
                    MethodInfo = service.ClientMethodInfo,
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
