using System;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;
using Uragano.Remoting;

namespace Sample.WebApi
{
    public class MyInterceptor1Attribute : InterceptorAttributeAbstract
    {
        public override async Task<ResultMessage> Intercept(IInterceptorContext context)
        {
            return await context.Next();
        }
    }
}
