using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;

namespace Sample.Common
{
    public class ClientMethodInterceptorAttribute : InterceptorAttributeAbstract
    {
        private ILogger Logger { get; }

        public ClientMethodInterceptorAttribute(ILogger<ServerInterceptorAttribute> logger)
        {
            Logger = logger;
        }

        public ClientMethodInterceptorAttribute()
        {
        }

        public override async Task<IServiceResult> Intercept(IInterceptorContext context)
        {
            Logger.LogTrace("\n------------------>Client method interceptor attribute\n");
            var r = await context.Next();
            return r;
        }
    }
}
