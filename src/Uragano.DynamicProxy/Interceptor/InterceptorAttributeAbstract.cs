using System;
using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Uragano.DynamicProxy.Interceptor
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method)]
    public abstract class InterceptorAttributeAbstract : Attribute, IInterceptor
    {

        public abstract Task<IServiceResult> Intercept(IInterceptorContext context);
    }
}
