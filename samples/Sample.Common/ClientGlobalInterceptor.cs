using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;

namespace Sample.Common
{
    public class ClientGlobalInterceptor : InterceptorAbstract
    {
        private ILogger Logger { get; }

        public ClientGlobalInterceptor(ILogger<ClientGlobalInterceptor> logger)
        {
            Logger = logger;
        }


        public override async Task<IServiceResult> Intercept(IInterceptorContext context)
        {

            Logger.LogTrace("\n---------------->Client global interceptor\n");
            return await context.Next();

        }
    }
}
