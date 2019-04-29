using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.Abstractions.Exceptions;
using Uragano.Remoting;
using Uragano.Abstractions.CircuitBreaker;
using System;
using System.Collections.Generic;

namespace Uragano.DynamicProxy.Interceptor
{
    public sealed class ClientDefaultInterceptor : InterceptorAbstract
    {
        private ILoadBalancing LoadBalancing { get; }
        private IClientFactory ClientFactory { get; }
        private ICircuitBreaker CircuitBreaker { get; }
        private UraganoSettings UraganoSettings { get; }

        public ClientDefaultInterceptor(ILoadBalancing loadBalancing, IClientFactory clientFactory, ICircuitBreaker circuitBreaker, UraganoSettings uraganoSettings)
        {
            LoadBalancing = loadBalancing;
            ClientFactory = clientFactory;
            CircuitBreaker = circuitBreaker;
            UraganoSettings = uraganoSettings;
        }

        public override async Task<IServiceResult> Intercept(IInterceptorContext context)
        {
            if (!(context is InterceptorContext ctx)) throw new ArgumentNullException(nameof(context));
            //No circuit breaker
            if (UraganoSettings.CircuitBreakerOptions == null)
            {
                return await Exec(ctx.ServiceName, ctx.ServiceRoute, ctx.Args, ctx.Meta);
            }
            //Circuit breaker
            return await CircuitBreaker.ExecuteAsync(ctx.ServiceRoute,
                async () => await Exec(ctx.ServiceName, ctx.ServiceRoute, ctx.Args, ctx.Meta), ctx.ReturnType);
        }

        private async Task<IServiceResult> Exec(string serviceName, string route, object[] args, Dictionary<string, string> meta)
        {
            var node = await LoadBalancing.GetNextNode(serviceName, route, args, meta);
            var client = await ClientFactory.CreateClientAsync(serviceName, node);
            var result = await client.SendAsync(new InvokeMessage(route, args, meta));
            if (result.Status != RemotingStatus.Ok)
                throw new RemoteInvokeException(route, result.Result?.ToString(), result.Status);
            return result;
        }
    }
}
