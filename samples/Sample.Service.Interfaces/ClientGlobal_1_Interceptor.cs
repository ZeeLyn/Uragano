using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;


namespace Sample.Service.Interfaces
{
    public class ClientGlobal_1_Interceptor : InterceptorAbstract
    {
        private ILogger Logger { get; }

        public ClientGlobal_1_Interceptor(ILogger<ClientGlobal_1_Interceptor> logger)
        {
            Logger = logger;
        }


        public override async Task<IServiceResult> Intercept(IInterceptorContext context)
        {
            Logger.LogDebug("\n---------------->Client global interceptor\n");
            return await context.Next();

        }
    }
}
