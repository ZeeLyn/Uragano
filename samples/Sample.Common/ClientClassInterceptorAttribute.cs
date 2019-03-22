using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;

namespace Sample.Common
{
    public class ClientClassInterceptorAttribute : InterceptorAttributeAbstract
    {
        private ILogger Logger { get; }

        public ClientClassInterceptorAttribute(ILogger<ServerInterceptorAttribute> logger)
        {
            Logger = logger;
        }

        public ClientClassInterceptorAttribute()
        {
        }

        public override async Task<IServiceResult> Intercept(IInterceptorContext context)
        {
            Logger.LogTrace("\n------------------>Client class interceptor attribute\n");
            var r = await context.Next();
            return r;
        }
    }
}
