using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;
using System;
using Microsoft.Extensions.Logging;

namespace Sample.Service.Interfaces
{
    public class ServerInterceptorAttribute : InterceptorAttributeAbstract
    {
        private ILogger Logger { get; }

        public ServerInterceptorAttribute()
        {

        }

        public ServerInterceptorAttribute(ILogger<ServerInterceptorAttribute> logger)
        {
            Logger = logger;
        }
        public override async Task<object> Intercept(IInterceptorContext context)
        {
            Logger.LogDebug("\n------------------>Server Interceptor attribute\n");
            return await context.Next();
        }
    }
}
