using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;

namespace Sample.Service.Interfaces
{
    public class ClientInterceptor_2_Attribute : InterceptorAttributeAbstract
    {
        public ClientInterceptor_2_Attribute() { }
        private ILogger Logger { get; }

        public ClientInterceptor_2_Attribute(ILogger<ClientInterceptor_2_Attribute> logger)
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
