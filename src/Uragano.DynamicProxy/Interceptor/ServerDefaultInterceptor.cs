using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.Service;

namespace Uragano.DynamicProxy.Interceptor
{
    public sealed class ServerDefaultInterceptor : InterceptorAbstract
    {
        private IServiceFactory ServiceFactory { get; }

        public ServerDefaultInterceptor(IServiceFactory serviceFactory)
        {
            ServiceFactory = serviceFactory;
        }

        public override async Task<IServiceResult> Intercept(IInterceptorContext context)
        {
            var service = ServiceFactory.Get(context.ServiceRoute);
            var instance = context.ServiceProvider.GetRequiredService(service.MethodInfo.DeclaringType);
            return new ServiceResult(await service.MethodInvoker.Invoke(instance, context.Args));
        }
    }
}
