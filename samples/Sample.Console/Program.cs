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
using Uragano.Core;
using Uragano.Remoting;


namespace Sample.Console
{
	class Program
	{
		public delegate int AddHandler(int id);
		static void Main(string[] args)
		{
			System.Console.WriteLine(IPHelper.GetLocalInternetIp());
			//System.Console.WriteLine(SpinWait.SpinUntil(() =>
			// {
			//	 System.Console.WriteLine(DateTime.Now);
			//	 return false;
			// }, 1000));

			//ThreadPool.SetMinThreads(100, 100);
			//var channel = CreateClient();
			//Parallel.For(1, 200, (index) =>
			// {

			//	 var t = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

			//	 System.Console.WriteLine("start:" + index);
			//	 ;
			//	 //t.Task.GetAwaiter().GetResult();
			//	 System.Console.WriteLine("ended");
			// });

			//Action<int> action = (i) => { };
			//action.BeginInvoke(1, null, null);
			//AsyncCallback callback = obj => { };
			//AddHandler handler = new AddHandler(Add);
			//var result = handler.BeginInvoke(1, callback, "asyncstat");

			//System.Console.WriteLine("继续执行");
			//System.Console.WriteLine(handler.EndInvoke(result));


			//ThreadPool.SetMinThreads(5, 5); // set min thread to 5
			//ThreadPool.SetMaxThreads(12, 12); // set max thread to 12

			//Stopwatch watch = new Stopwatch();
			//watch.Start();

			//WaitCallback callback = index =>
			//{
			//	System.Console.WriteLine(String.Format("{0}: Task {1} started", watch.Elapsed, index));
			//	Thread.Sleep(10000);
			//	System.Console.WriteLine(String.Format("{0}: Task {1} finished", watch.Elapsed, index));
			//};

			//for (int i = 0; i < 20; i++)
			//{
			//	ThreadPool.QueueUserWorkItem(callback, i);
			//}


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

		public static int Add(int a)
		{
			System.Console.WriteLine("\n开始计算：" + a);
			Thread.Sleep(10000); //模拟该方法运行三秒
			System.Console.WriteLine("计算完成！");
			return a;
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
