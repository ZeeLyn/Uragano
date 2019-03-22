using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;

namespace Sample.Common
{
    public class ServerGlobalInterceptor : InterceptorAbstract
    {
        private ILogger Logger { get; }

        public ServerGlobalInterceptor(ILogger<ServerGlobalInterceptor> logger)
        {
            Logger = logger;
        }


        public override async Task<IServiceResult> Intercept(IInterceptorContext context)
        {

            Logger.LogTrace("\n---------------->Server global interceptor\n");
            return await context.Next();

        }
    }
}
