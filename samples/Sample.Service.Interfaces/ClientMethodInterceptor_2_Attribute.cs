using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;

namespace Sample.Service.Interfaces
{
    public class ClientMethodInterceptor_2_Attribute : InterceptorAttributeAbstract
    {
        public ClientMethodInterceptor_2_Attribute() { }
        private ILogger Logger { get; }

        public ClientMethodInterceptor_2_Attribute(ILogger<ClientMethodInterceptor_2_Attribute> logger)
        {
            Logger = logger;
        }
        public override async Task<object> Intercept(IInterceptorContext context)
        {
            Logger.LogDebug("\n--------------->Client interceptor attribute\n");
            return await context.Next();
        }
    }
}
