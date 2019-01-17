using System;
using System.Collections.Concurrent;
using System.Net;
using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;

namespace Uragano.Remoting
{
	public class ClientFactory : IClientFactory
	{

		private readonly ConcurrentDictionary<string, Lazy<IClient>> _clients = new ConcurrentDictionary<string, Lazy<IClient>>();

		public IClient CreateClient(string host, int port)
		{
			var key = $"{host}:{port}";
			try
			{
				return _clients.GetOrAdd(key, new Lazy<IClient>(() =>
				{
					var bootstrap = ClientFactory.CreateBootstrap();
					return new Client();
				})).Value;
			}
			catch
			{
				_clients.TryRemove(key, out _);
				throw;
			}
		}

		public static Bootstrap CreateBootstrap()
		{
			IEventLoopGroup group;

			var bootstrap = new Bootstrap();
			if (false)
			{
				group = new EventLoopGroup();
				bootstrap.Channel<TcpServerChannel>();
			}
			else
			{
				group = new MultithreadEventLoopGroup();
				bootstrap.Channel<TcpServerSocketChannel>();
			}
			bootstrap
				.Channel<TcpSocketChannel>()
				.Option(ChannelOption.TcpNodelay, true)
				.Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
				.Group(group);

			return bootstrap;
		}


	}
}
