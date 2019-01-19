using System;
using System.Net;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceInvoker;
using Uragano.Codec.MessagePack;

namespace Uragano.Remoting
{
	public class ServerBootstrap : IBootstrap, IDisposable
	{
		private IChannel Channel { get; set; }

		private IProxyGenerateFactory ProxyGenerateFactory { get; }

		private IInvokerFactory InvokerFactory { get; }

		private ServerSettings ServerSettings { get; }

		private IServiceProvider ServiceProvider { get; }

		public ServerBootstrap(IInvokerFactory invokerFactory, IServiceProvider serviceProvider, IProxyGenerateFactory proxyGenerateFactory, UraganoSettings uraganoSettings)
		{

			InvokerFactory = invokerFactory;
			ServiceProvider = serviceProvider;
			ProxyGenerateFactory = proxyGenerateFactory;
			ServerSettings = uraganoSettings.ServerSettings;
		}

		public async Task StartAsync()
		{

			IEventLoopGroup bossGroup;
			IEventLoopGroup workerGroup;
			var bootstrap = new DotNetty.Transport.Bootstrapping.ServerBootstrap();
			if (ServerSettings.Libuv)
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
					if (ServerSettings.X509Certificate2 != null)
					{
						pipeline.AddLast(TlsHandler.Server(ServerSettings.X509Certificate2));
					}
					pipeline.AddLast(new LoggingHandler("SRV-CONN"));
					pipeline.AddLast(new LengthFieldPrepender(2));
					pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 2, 0, 2));
					pipeline.AddLast(new MessageDecoder<InvokeMessage>());
					pipeline.AddLast(new MessageEncoder<ResultMessage>());
					pipeline.AddLast(new ServerMessageHandler(InvokerFactory, ProxyGenerateFactory));
				}));

			Channel = await bootstrap.BindAsync(new IPEndPoint(ServerSettings.IP, ServerSettings.Port));
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
