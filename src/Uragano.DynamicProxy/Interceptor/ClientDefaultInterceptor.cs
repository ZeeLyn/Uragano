using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.Abstractions.Exceptions;
using Uragano.Codec.MessagePack;
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

        public override async Task<object> Intercept(IInterceptorContext context)
        {
            if (context is InterceptorContext ctx)
            {
                if (UraganoSettings.CircuitBreakerOptions != null)
                {
                    if (ctx.ReturnType != null)
                        return await CircuitBreaker.ExecuteAsync(ctx.ServiceRoute,
                            async () => await Exec(ctx.ServiceName, ctx.ServiceRoute, ctx.Args, ctx.Meta,
                                ctx.ReturnType), ctx.ReturnType);

                    await CircuitBreaker.ExecuteAsync(ctx.ServiceRoute, async () =>
                    {
                        await Exec(ctx.ServiceName, ctx.ServiceRoute, ctx.Args, ctx.Meta, null);
                    });
                    return null;
                }

                return await Exec(ctx.ServiceName, ctx.ServiceRoute, ctx.Args, ctx.Meta, ctx.ReturnType);
            }

            throw new ArgumentNullException(nameof(context));
        }

        private async Task<object> Exec(string serviceName, string route, object[] args, Dictionary<string, string> meta, Type returnValueType)
        {
            ServiceNodeInfo node;
            if (UraganoSettings.IsDevelopment)
                node = new ServiceNodeInfo { Address = UraganoSettings.ServerSettings.IP.ToString(), Port = UraganoSettings.ServerSettings.Port };
            else
                node = await LoadBalancing.GetNextNode(serviceName);
            var client = await ClientFactory.CreateClientAsync(node.Address, node.Port);
            var result = await client.SendAsync(new InvokeMessage
            {
                Args = args,
                Route = route,
                Meta = meta
            });

            if (result.Status != RemotingStatus.Ok)
                throw new RemoteInvokeException(route, result.Result.ToString());
            if (returnValueType == null)
                return null;
            if (result.Result == null)
                return default;
            return SerializerHelper.Deserialize(SerializerHelper.Serialize(result.Result), returnValueType);
        }
    }
}
