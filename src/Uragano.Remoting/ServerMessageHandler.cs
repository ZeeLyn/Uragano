using System;
using DotNetty.Transport.Channels;
using Uragano.Abstractions.ServiceInvoker;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;

namespace Uragano.Remoting
{
	public class ServerMessageHandler : ChannelHandlerAdapter
	{

		private IInvokerFactory InvokerFactory { get; }

		private IServiceProvider ServiceProvider { get; }

		public ServerMessageHandler(IInvokerFactory invokerFactory, IServiceProvider serviceProvider)
		{
			InvokerFactory = invokerFactory;
			ServiceProvider = serviceProvider;
		}

		public override void ChannelRead(IChannelHandlerContext context, object message)
		{
			if (!(message is TransportMessage<InvokeMessage> transportMessage))
				throw new ArgumentNullException(nameof(message));
			try
			{
				var result = InvokerFactory.Invoke(transportMessage.Body.Route, transportMessage.Body.Args, transportMessage.Body.Meta).GetAwaiter().GetResult();

				context.WriteAndFlushAsync(new TransportMessage<ResultMessage>
				{
					Id = transportMessage.Id,
					Body = new ResultMessage(result)
				}).GetAwaiter().GetResult();
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
