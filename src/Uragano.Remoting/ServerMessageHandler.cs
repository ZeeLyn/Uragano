using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Uragano.Abstractions.ServiceInvoker;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;

namespace Uragano.Remoting
{
	public class ServerMessageHandler : ChannelHandlerAdapter
	{

		private IInvokerFactory InvokerFactory { get; }
		private IProxyGenerateFactory ProxyGenerateFactory { get; }

		public ServerMessageHandler(IInvokerFactory invokerFactory, IProxyGenerateFactory proxyGenerateFactory)
		{
			InvokerFactory = invokerFactory;
			ProxyGenerateFactory = proxyGenerateFactory;
		}

		public override void ChannelRead(IChannelHandlerContext context, object message)
		{
			if (!(message is TransportMessage<InvokeMessage> transportMessage))
				throw new ArgumentNullException(nameof(message));
			try
			{
				using (var scope = ContainerManager.CreateScope())
				{
					var service = InvokerFactory.Get(transportMessage.Body.Route);

					var proxyInstance = ProxyGenerateFactory.CreateLocalProxy(service.MethodInfo.DeclaringType);
					var result = service.MethodInfo.Invoke(proxyInstance, transportMessage.Body.Args);

					context.WriteAndFlushAsync(new TransportMessage<ResultMessage>
					{
						Id = transportMessage.Id,
						Body = new ResultMessage(result)
					}).GetAwaiter().GetResult();
				}
			}
			catch (Exception e)
			{
				context.WriteAndFlushAsync(new TransportMessage<ResultMessage>
				{
					Id = transportMessage.Id,
					Body = new ResultMessage(e.Message) { Status = RemotingStatus.Error }
				}).Wait();
			}
		}

		public override void ChannelReadComplete(IChannelHandlerContext context)
		{
			context.Flush();
		}

		public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
		{
			context.CloseAsync().Wait();
		}
	}
}
