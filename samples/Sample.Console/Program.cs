using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Concurrency;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Uragano.Abstractions;
using Uragano.Codec.MessagePack;
using Uragano.Remoting;


namespace Sample.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			int count = 0;
			ThreadPool.SetMinThreads(100, 100);
			//var channel = CreateClient();
			Parallel.For(1, 200, (index) =>
		  {


			  var t = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
			  count++;
			  System.Console.WriteLine("start:" + count);
			  t.Task.Wait();



			  System.Console.WriteLine("ended");

			  //var r = await Task.Run(async () =>
			  //{
			  // System.Console.WriteLine("start");
			  // var t = new TaskCompletionSource<string>();
			  // return await t.Task;
			  // System.Console.WriteLine("ended");
			  //});
		  });


			System.Console.ReadKey();
		}

		private static IChannel CreateClient()
		{
			IEventLoopGroup group;

			var Bootstrap = new Bootstrap();
			if (UraganoOptions.DotNetty_Enable_Libuv.Value)
			{
				group = new EventLoopGroup();
				Bootstrap.Channel<TcpChannel>();
			}
			else
			{
				group = new MultithreadEventLoopGroup();
				Bootstrap.Channel<TcpSocketChannel>();
			}

			Bootstrap
				.Group(group)
				.Option(ChannelOption.TcpNodelay, true)
				.Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
				.Option(ChannelOption.SoBacklog, 100)
				.Option(ChannelOption.ConnectTimeout, UraganoOptions.DotNetty_Connect_Timeout.Value)
				.Handler(new ActionChannelInitializer<IChannel>(c =>
				{
					var pipeline = c.Pipeline;
					//pipeline.AddLast(new LoggingHandler("SRV-CONN"));
					pipeline.AddLast(new LengthFieldPrepender(4));
					pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
					pipeline.AddLast(new MessageDecoder<ResultMessage>());
					pipeline.AddLast(new MessageEncoder<InvokeMessage>());
					pipeline.AddLast(new ClientMessageHandler());
				}));
			return Bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("192.168.1.254"), 5001)).GetAwaiter().GetResult();
		}
		internal class ClientMessageHandler : ChannelHandlerAdapter
		{



			public override void ChannelRead(IChannelHandlerContext context, object message)
			{
				//var msg = message as TransportMessage<ResultMessage>;
				System.Console.WriteLine("收到消息");
			}

			public override void ChannelInactive(IChannelHandlerContext context)
			{

			}
		}
	}



	public class A
	{
		public object Data { get; set; }
	}

	public class B
	{
		public string Name { get; set; }

		public int Age { get; set; }
	}
}
