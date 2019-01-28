using System;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Uragano.Abstractions.ServiceInvoker;
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
			Task.Run(async () =>
			{
				var msg = message;
				if (!(msg is TransportMessage<InvokeMessage> transportMessage))
					throw new ArgumentNullException(nameof(message));
				try
				{
					var result = await InvokerFactory.Invoke(transportMessage.Body.Route, transportMessage.Body.Args, transportMessage.Body.Meta);
					await context.WriteAndFlushAsync(new TransportMessage<ResultMessage>
					{
						Id = transportMessage.Id,
						Body = new ResultMessage(result)
					});
				}
				catch (Exception e)
				{
					await context.WriteAndFlushAsync(new TransportMessage<ResultMessage>
					{
						Id = transportMessage.Id,
						Body = new ResultMessage(e.Message) { Status = RemotingStatus.Error }
					});
				}
			});
		}

		public override void ChannelReadComplete(IChannelHandlerContext context)
		{
			context.Flush();
		}

		public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
		{
			context.CloseAsync();
		}
	}
}
