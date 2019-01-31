using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.Abstractions.Exceptions;
using Uragano.Codec.MessagePack;
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

        public override async Task<object> Intercept(IInterceptorContext context)
        {
            var ctx = context as InterceptorContext;

            if (UraganoSettings.CircuitBreakerOptions != null)
            {
                if (ctx.ReturnType == null)
                {
                    await CircuitBreaker.ExecuteAsync(ctx.ServiceRoute, async () =>
                    {
                        var node = await LoadBalancing.GetNextNode(ctx.ServiceName);
                        var client = await ClientFactory.CreateClientAsync(node.Address, node.Port);
                        var result = await client.SendAsync(new InvokeMessage
                        {
                            Args = ctx.Args,
                            Route = ctx.ServiceRoute,
                            Meta = ctx.Meta
                        });

                        if (result.Status != RemotingStatus.Ok)
                            throw new RemoteInvokeException(ctx.ServiceRoute, result.Result.ToString());
                    });
                    return null;
                }
                else
                {
                    return await CircuitBreaker.ExecuteAsync(ctx.ServiceRoute, async () =>
                    {
                        return await Exec(ctx.ServiceName, ctx.ServiceRoute, ctx.Args, ctx.Meta, ctx.ReturnType);
                    }, ctx.ReturnType);
                }
            }
            else
            {
                return await Exec(ctx.ServiceName, ctx.ServiceRoute, ctx.Args, ctx.Meta, ctx.ReturnType);
            }
        }

        private async Task<object> Exec(string serviceName, string route, object[] args, Dictionary<string, string> meta, Type returnValueType)
        {
            var node = await LoadBalancing.GetNextNode(serviceName);
            var client = await ClientFactory.CreateClientAsync(node.Address, node.Port);
            var result = await client.SendAsync(new InvokeMessage
            {
                Args = args,
                Route = route,
                Meta = meta
            });

            if (result.Status != RemotingStatus.Ok)
                throw new RemoteInvokeException(route, result.Result.ToString());
            if (result.Result == null)
                return default;
            return SerializerHelper.Deserialize(SerializerHelper.Serialize(result.Result), returnValueType);
        }
    }
}
