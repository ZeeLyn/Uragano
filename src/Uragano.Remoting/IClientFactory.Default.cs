using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;


namespace Uragano.Remoting
{
    public class ClientFactory : IClientFactory
    {
        private readonly ConcurrentDictionary<(string, int), Task<IClient>> _clients = new ConcurrentDictionary<(string, int), Task<IClient>>();

        private static readonly AttributeKey<TransportContext> TransportContextAttributeKey = AttributeKey<TransportContext>.ValueOf(typeof(ClientFactory), nameof(TransportContext));

        private static readonly AttributeKey<IMessageListener> MessageListenerAttributeKey = AttributeKey<IMessageListener>.ValueOf(typeof(ClientFactory), nameof(IMessageListener));

        private ICodec Codec { get; }

        private ILogger Logger { get; }

        public ClientFactory(ICodec codec, ILogger<ClientFactory> logger)
        {
            Codec = codec;
            Logger = logger;
        }

        public void RemoveClient(string host, int port)
        {
            if (!_clients.TryRemove((host, port), out var client)) return;
            client.Result.Dispose();
        }

        public async Task<IClient> CreateClientAsync(string host, int port)
        {
            var key = (host, port);
            try
            {
                return await _clients.GetOrAdd(key, async k =>
                {
                    IEventLoopGroup group;
                    var bootstrap = new Bootstrap();
                    if (UraganoOptions.DotNetty_Enable_Libuv.Value)
                    {
                        group = new EventLoopGroup();
                        bootstrap.Channel<TcpChannel>();
                    }
                    else
                    {
                        group = new MultithreadEventLoopGroup();
                        bootstrap.Channel<TcpSocketChannel>();
                    }

                    bootstrap
                        .Group(group)
                        .Option(ChannelOption.TcpNodelay, true)
                        .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                        .Option(ChannelOption.ConnectTimeout, UraganoOptions.DotNetty_Connect_Timeout.Value)
                        .Handler(new ActionChannelInitializer<IChannel>(ch =>
                        {
                            var pipeline = ch.Pipeline;
                            //if (ServerSettings.X509Certificate2 != null)
                            //{
                            //pipeline.AddFirst(new TlsHandler(new ClientTlsSettings());
                            //}
                            //pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                            pipeline.AddLast(new LengthFieldPrepender(4));
                            pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                            pipeline.AddLast(new MessageDecoder<IServiceResult>(Codec));
                            pipeline.AddLast(new MessageEncoder<IInvokeMessage>(Codec));
                            pipeline.AddLast(new ClientMessageHandler(this));
                        }));

                    EndPoint endPoint;
                    if (IPAddress.TryParse(host, out var ip))
                        endPoint = new IPEndPoint(ip, port);
                    else
                        endPoint = new DnsEndPoint(host, port);
                    var channel = await bootstrap.ConnectAsync(endPoint);
                    channel.GetAttribute(TransportContextAttributeKey).Set(new TransportContext
                    {
                        Host = host,
                        Port = port
                    });
                    var listener = new MessageListener();
                    channel.GetAttribute(MessageListenerAttributeKey).Set(listener);
                    return new Client(channel, group, listener, Logger);
                });
            }
            catch
            {
                _clients.TryRemove(key, out _);
                throw;
            }
        }


        public void Dispose()
        {
            foreach (var client in _clients.Values.Select(p => p.Result))
            {
                client.DisconnectAsync().GetAwaiter().GetResult();
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
                var msg = message as TransportMessage<IServiceResult>;
                var listener = context.Channel.GetAttribute(MessageListenerAttributeKey).Get();
                listener.Received(msg);
            }

            public override void ChannelInactive(IChannelHandlerContext context)
            {
                var ctx = context.Channel.GetAttribute(TransportContextAttributeKey).Get();
                ClientFactory.RemoveClient(ctx.Host, ctx.Port);
            }
        }
    }
}
