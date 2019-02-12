using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;
using Uragano.Remoting;

namespace Sample.Service.Interfaces
{
    public class ClientInterceptor_1_Attribute : InterceptorAttributeAbstract
    {
        public ClientInterceptor_1_Attribute() { }
        private ILogger Logger { get; }

        public ClientInterceptor_1_Attribute(ILogger<ClientInterceptor_1_Attribute> logger)
        {
            Logger = logger;
        }
        public override async Task<ResultMessage> Intercept(IInterceptorContext context)
        {
            Logger.LogDebug("\n--------------->Client interceptor attribute\n");
            return await context.Next();
        }
    }
}
