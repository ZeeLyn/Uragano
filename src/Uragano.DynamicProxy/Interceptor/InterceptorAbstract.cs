using System.Threading.Tasks;
using Uragano.Abstractions;


namespace Uragano.DynamicProxy.Interceptor
{
    public abstract class InterceptorAbstract : IInterceptor
    {
        public abstract Task<IServiceResult> Intercept(IInterceptorContext context);
    }
}
