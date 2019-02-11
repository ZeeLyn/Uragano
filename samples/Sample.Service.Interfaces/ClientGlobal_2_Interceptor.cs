using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;
using Uragano.Remoting;

namespace Sample.Service.Interfaces
{
    public class ClientGlobal_2_Interceptor : InterceptorAbstract
    {
        private ILogger Logger { get; }

        public ClientGlobal_2_Interceptor(ILogger<ClientGlobal_2_Interceptor> logger)
        {
            Logger = logger;
        }


        public override async Task<ResultMessage> Intercept(IInterceptorContext context)
        {
            Logger.LogDebug("\n---------------->Client global interceptor\n");
            return await context.Next();
        }
    }
}
