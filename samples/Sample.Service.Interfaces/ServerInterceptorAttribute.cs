using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;
using System;

namespace Sample.Service.Interfaces
{
    public class ServerInterceptorAttribute : InterceptorAttributeAbstract
    {
        public override async Task<object> Intercept(IInterceptorContext context)
        {
            Console.WriteLine("Server Interceptor");
            return await context.Next();
        }
    }
}
