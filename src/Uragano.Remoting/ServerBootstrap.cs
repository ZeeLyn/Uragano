using System;
using System.Net;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Uragano.Abstractions.Remoting;
using Uragano.Abstractions.ServiceInvoker;
using Uragano.Codec.MessagePack;

namespace Uragano.Remoting
{
	public class ServerBootstrap : IBootstrap, IDisposable
	{
		private IChannel Channel { get; set; }



		private IInvokerFactory InvokerFactory { get; }

		private IServiceProvider ServiceProvider { get; }

		public ServerBootstrap(IInvokerFactory invokerFactory, IServiceProvider serviceProvider)
		{

			InvokerFactory = invokerFactory;
			ServiceProvider = serviceProvider;
		}

		public async Task StartAsync(string host, int port)
		{
			var Libuv = false;
			IEventLoopGroup bossGroup;
			IEventLoopGroup workerGroup;
			var bootstrap = new DotNetty.Transport.Bootstrapping.ServerBootstrap();
			if (Libuv)
			{
				var dispatcher = new DispatcherEventLoopGroup();
				bossGroup = dispatcher;
				workerGroup = new WorkerEventLoopGroup(dispatcher);
				bootstrap.Channel<TcpServerChannel>();
			}
			else
			{
				bossGroup = new MultithreadEventLoopGroup(1);
				workerGroup = new MultithreadEventLoopGroup();
				bootstrap.Channel<TcpServerSocketChannel>();
			}

			bootstrap
				.Group(bossGroup, workerGroup)
				.Option(ChannelOption.SoBacklog, 100)
				.ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
				.ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
				{
					var pipeline = channel.Pipeline;
					pipeline.AddLast(new LoggingHandler("SRV-CONN"));
					pipeline.AddLast(new LengthFieldPrepender(2));
					pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 2, 0, 2));
					pipeline.AddLast(new MessageDecoder<InvokeMessage>());
					pipeline.AddLast(new MessageEncoder<ResultMessage>());
					pipeline.AddLast(new ServerMessageHandler(InvokerFactory, ServiceProvider));
				}));
			Channel = await bootstrap.BindAsync(new IPEndPoint(IPAddress.Parse(host), port));
		}


		public async Task StopAsync()
		{
			await Channel.EventLoop.ShutdownGracefullyAsync();
			await Channel.CloseAsync();
		}

		public void Dispose()
		{
			Channel.DisconnectAsync().Wait();
		}
	}
}
