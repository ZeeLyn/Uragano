using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.Abstractions.Exceptions;
using Uragano.Remoting;
using Uragano.Abstractions.CircuitBreaker;
using System;
using System.Collections.Generic;
using Uragano.Abstractions.ServiceDiscovery;

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
                if (ctx.ReturnType != null)
                    return new ServiceResult(await Exec(ctx.ServiceName, ctx.ServiceRoute, ctx.Args, ctx.Meta,
                        ctx.ReturnType));
                await Exec(ctx.ServiceName, ctx.ServiceRoute, ctx.Args, ctx.Meta, null);
                return new ServiceResult(null);
            }
            //Circuit breaker
            if (ctx.ReturnType != null)
                return new ServiceResult(await CircuitBreaker.ExecuteAsync(ctx.ServiceRoute,
                    async () => await Exec(ctx.ServiceName, ctx.ServiceRoute, ctx.Args, ctx.Meta,
                        ctx.ReturnType), ctx.ReturnType));

            await CircuitBreaker.ExecuteAsync(ctx.ServiceRoute,
                async () => { await Exec(ctx.ServiceName, ctx.ServiceRoute, ctx.Args, ctx.Meta, null); });
            return new ServiceResult(null);
        }

        private async Task<object> Exec(string serviceName, string route, object[] args, Dictionary<string, string> meta, Type returnValueType)
        {
            ServiceNodeInfo node;
            if (UraganoSettings.IsDevelopment)
                node = new ServiceNodeInfo { Address = UraganoSettings.ServerSettings.IP.ToString(), Port = UraganoSettings.ServerSettings.Port };
            else
                node = await LoadBalancing.GetNextNode(serviceName);
            var client = await ClientFactory.CreateClientAsync(node.Address, node.Port);
            var result = await client.SendAsync(new InvokeMessage(route, args, meta));
            if (result.Status != RemotingStatus.Ok)
                throw new RemoteInvokeException(route, result.Result?.ToString(), result.Status);
            if (returnValueType == null)
                return null;
            return result.Result;
        }
    }
}
