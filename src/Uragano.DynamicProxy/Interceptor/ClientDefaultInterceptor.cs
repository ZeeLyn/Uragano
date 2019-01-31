using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.Abstractions.Exceptions;
using Uragano.Codec.MessagePack;
using Uragano.Remoting;

namespace Uragano.DynamicProxy.Interceptor
{
    public class ClientDefaultInterceptor : InterceptorAbstract
    {
        private ILoadBalancing LoadBalancing { get; }
        private IClientFactory ClientFactory { get; }
        public ClientDefaultInterceptor(ILoadBalancing loadBalancing, IClientFactory clientFactory)
        {
            LoadBalancing = loadBalancing;
            ClientFactory = clientFactory;
        }

        public override async Task<object> Intercept(IInterceptorContext context)
        {
            var ctx = context as InterceptorContext;
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
            if (result.Result == null)
                return default;
            return SerializerHelper.Deserialize(SerializerHelper.Serialize(result.Result), ctx.ReturnType);
        }
    }
}
