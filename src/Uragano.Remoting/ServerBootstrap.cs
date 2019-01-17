using System;
using System.Net;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;

namespace Uragano.Remoting
{
	public class ServerBootstrap : IBootstrap, IDisposable
	{
		private IChannel Channel { get; set; }

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
				.ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
				{
					var pipeline = channel.Pipeline;
					pipeline.AddLast(new LengthFieldPrepender(4));
					pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
					pipeline.AddLast(new DecodChannelHandlerAdapter());
					pipeline.AddLast(new ServerMessageHandler());
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
