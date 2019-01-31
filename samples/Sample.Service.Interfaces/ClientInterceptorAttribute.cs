using System;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;

namespace Sample.Service.Interfaces
{
    public class ClientInterceptorAttribute : InterceptorAttributeAbstract
    {
        public override async Task<object> Intercept(IInterceptorContext context)
        {
            Console.WriteLine("Client interceptor");
            return await context.Next();
        }
    }
}
