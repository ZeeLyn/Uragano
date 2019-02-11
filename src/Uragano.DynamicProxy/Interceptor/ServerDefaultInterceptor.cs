using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceInvoker;
using Uragano.Remoting;

namespace Uragano.DynamicProxy.Interceptor
{
    public sealed class ServerDefaultInterceptor : InterceptorAbstract
    {
        private IInvokerFactory InvokerFactory { get; }

        public ServerDefaultInterceptor(IInvokerFactory invokerFactory)
        {
            InvokerFactory = invokerFactory;
        }

        public override async Task<IServiceResult<object>> Intercept(IInterceptorContext context)
        {
            var service = InvokerFactory.Get(context.ServiceRoute);
            var instance = context.ServiceProvider.GetRequiredService(service.MethodInfo.DeclaringType);
            return new ServiceResult<object>(await service.MethodInvoker.Invoke(instance, context.Args));
        }
    }
}
