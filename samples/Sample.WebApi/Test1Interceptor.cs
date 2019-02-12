using System;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;
using Uragano.Remoting;

namespace Sample.WebApi
{
    public class Test1Interceptor : InterceptorAbstract
    {
        public override async Task<ResultMessage> Intercept(IInterceptorContext context)
        {
            Console.WriteLine("---------------->Interceptor1");
            return await context.Next();
        }
    }
}
