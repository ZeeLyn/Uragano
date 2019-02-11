using System;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.Remoting;

namespace Uragano.DynamicProxy.Interceptor
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method)]
    public abstract class InterceptorAttributeAbstract : Attribute, IInterceptor
    {
        public abstract Task<ResultMessage> Intercept(IInterceptorContext context);
    }
}
