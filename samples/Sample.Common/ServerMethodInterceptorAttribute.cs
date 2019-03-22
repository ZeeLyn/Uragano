using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;

namespace Sample.Common
{
    public class ServerMethodInterceptorAttribute : InterceptorAttributeAbstract
    {
        private ILogger Logger { get; }

        public ServerMethodInterceptorAttribute(ILogger<ServerInterceptorAttribute> logger)
        {
            Logger = logger;
        }

        public ServerMethodInterceptorAttribute()
        {
        }

        public override async Task<IServiceResult> Intercept(IInterceptorContext context)
        {
            Logger.LogTrace("\n------------------>Server method interceptor attribute\n");
            var r = await context.Next();
            return r;
        }
    }
}
