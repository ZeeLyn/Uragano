using System.Threading.Tasks;
using Uragano.Abstractions.Exceptions;
using Uragano.Abstractions.LoadBalancing;
using Uragano.Codec.MessagePack;
using Uragano.Abstractions;
using Uragano.Remoting;

namespace Uragano.DynamicProxy
{
	public class RemotingInvoke : IRemotingInvoke
	{
		private ILoadBalancing LoadBalancing { get; }
		private IClientFactory ClientFactory { get; }

		public RemotingInvoke(ILoadBalancing loadBalancing, IClientFactory clientFactory)
		{
			LoadBalancing = loadBalancing;
			ClientFactory = clientFactory;
		}
		public async Task<T> InvokeAsync<T>(object[] args, string route, string serviceName)
		{
			var node = LoadBalancing.GetNextNode(serviceName);
			var client = ClientFactory.CreateClient(node.Address, node.Port);
			var result = await client.SendAsync(new InvokeMessage
			{
				Args = args,
				Route = route
			});

			if (result.Status != RemotingStatus.Ok)
				throw new RemoteInvokeException(route, result.Result.ToString());
			if (result.Result == null)
				return default;
			return (T)SerializerHelper.Deserialize(SerializerHelper.Serialize(result.Result), typeof(T));
		}
	}
}
