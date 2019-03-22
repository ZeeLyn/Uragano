using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;

namespace Sample.Common
{
    public class ServerClassInterceptorAttribute : InterceptorAttributeAbstract
    {
        private ILogger Logger { get; }

        public ServerClassInterceptorAttribute(ILogger<ServerInterceptorAttribute> logger)
        {
            Logger = logger;
        }

        public ServerClassInterceptorAttribute()
        {
        }

        public override async Task<IServiceResult> Intercept(IInterceptorContext context)
        {
            Logger.LogTrace("\n------------------>Server class interceptor attribute\n");
            var r = await context.Next();
            return r;
        }
    }
}
