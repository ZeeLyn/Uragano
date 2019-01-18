using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Uragano.Abstractions.ServiceInvoker;
using Uragano.Abstractions.Remoting;
using Microsoft.Extensions.DependencyInjection;

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
			var tranMsg = message as TransportMessage<InvokeMessage>;
			var service = InvokerFactory.Get(tranMsg.Content.Route);
			object result;
			using (var scope = ServiceProvider.CreateScope())
			{
				result = service.MethodInvoker.Invoke(scope.ServiceProvider.GetService(service.MethodInfo.DeclaringType),
				   tranMsg.Content.Args);
			}
			context.WriteAndFlushAsync(new TransportMessage<ResultMessage>
			{
				Id = tranMsg.Id,
				Content = new ResultMessage
				{
					Result = result
				}
			}).Wait();

		}

		public override void ChannelReadComplete(IChannelHandlerContext context)
		{
			context.Flush();
		}

		public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
		{
			Console.Write("出现错误：" + exception.Message);
			context.CloseAsync().Wait();
		}
	}
}
