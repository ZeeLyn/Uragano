using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;

namespace Sample.Common
{
    public class ServerInterceptorAttribute : InterceptorAttributeAbstract
    {
        private ILogger Logger { get; }

        public ServerInterceptorAttribute(ILogger<ServerInterceptorAttribute> logger)
        {
            Logger = logger;
        }

        public ServerInterceptorAttribute()
        {
        }

        public override async Task<IServiceResult> Intercept(IInterceptorContext context)
        {
            Logger.LogTrace("\n------------------>Server Interceptor attribute\n");
            var r = await context.Next();
            return r;
        }
    }
}
