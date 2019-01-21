using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Uragano.Abstractions;
using Uragano.Codec.MessagePack;

namespace Uragano.Remoting
{
	public class ClientFactory : IClientFactory
	{
		private Bootstrap Bootstrap { get; }

		private readonly ConcurrentDictionary<(string, int), Lazy<IClient>> _clients = new ConcurrentDictionary<(string, int), Lazy<IClient>>();

		private static readonly AttributeKey<TransportContext> TransportContextAttributeKey = AttributeKey<TransportContext>.ValueOf(typeof(ClientFactory), nameof(TransportContext));

		private static readonly AttributeKey<IMessageListener> MessageListenerAttributeKey = AttributeKey<IMessageListener>.ValueOf(typeof(ClientFactory), nameof(IMessageListener));


		public ClientFactory()
		{
			IEventLoopGroup group;

			Bootstrap = new Bootstrap();
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
				.Option(ChannelOption.ConnectTimeout, UraganoOptions.DotNetty_Connect_Timeout.Value)
				.Handler(new ActionChannelInitializer<IChannel>(channel =>
				{
					var pipeline = channel.Pipeline;
					//pipeline.AddLast(new LoggingHandler("SRV-CONN"));
					pipeline.AddLast(new LengthFieldPrepender(4));
					pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
					pipeline.AddLast(new MessageDecoder<ResultMessage>());
					pipeline.AddLast(new MessageEncoder<InvokeMessage>());
					pipeline.AddLast(new ClientMessageHandler(this));
				}));
		}

		public void RemoveClient(string host, int port)
		{
			if (!_clients.TryRemove((host, port), out var client)) return;
			if (client.IsValueCreated)
				client.Value.Dispose();
		}

		public IClient CreateClient(string host, int port)
		{
			var key = (host, port);
			try
			{
				return _clients.GetOrAdd(key, new Lazy<IClient>(() =>
				 {
					 var bootstrap = Bootstrap;
					 EndPoint endPoint;
					 if (IPAddress.TryParse(host, out var ip))
						 endPoint = new IPEndPoint(ip, port);
					 else
						 endPoint = new DnsEndPoint(host, port);
					 var channel = bootstrap.ConnectAsync(endPoint).GetAwaiter().GetResult();
					 channel.GetAttribute(TransportContextAttributeKey).Set(new TransportContext
					 {
						 Host = host,
						 Port = port
					 });
					 var listener = new MessageListener();
					 channel.GetAttribute(MessageListenerAttributeKey).Set(listener);
					 return new Client(channel, listener);
				 })).Value;
			}
			catch
			{
				_clients.TryRemove(key, out _);
				throw;
			}
		}


		public void Dispose()
		{
			foreach (var client in _clients.Values.Where(p => p.IsValueCreated))
			{
				client.Value.Dispose();
			}
		}

		internal class ClientMessageHandler : ChannelHandlerAdapter
		{

			private IClientFactory ClientFactory { get; }


			public ClientMessageHandler(IClientFactory clientFactory)
			{
				ClientFactory = clientFactory;
			}

			public override void ChannelRead(IChannelHandlerContext context, object message)
			{
				var msg = message as TransportMessage<ResultMessage>;
				var listener = context.Channel.GetAttribute(MessageListenerAttributeKey).Get();
				listener.Received(new MessageSender(), msg);
			}

			public override void ChannelInactive(IChannelHandlerContext context)
			{
				var ctx = context.Channel.GetAttribute(TransportContextAttributeKey).Get();
				ClientFactory.RemoveClient(ctx.Host, ctx.Port);
			}
		}
	}
}
