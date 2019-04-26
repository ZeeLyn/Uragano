using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;


namespace Uragano.Remoting
{
    public class ClientFactory : IClientFactory
    {
        private readonly ConcurrentDictionary<(string, int), Task<IClient>> _clients = new ConcurrentDictionary<(string, int), Task<IClient>>();

        private static readonly AttributeKey<TransportContext> TransportContextAttributeKey = AttributeKey<TransportContext>.ValueOf(typeof(ClientFactory), nameof(TransportContext));

        private static readonly AttributeKey<IMessageListener> MessageListenerAttributeKey = AttributeKey<IMessageListener>.ValueOf(typeof(ClientFactory), nameof(IMessageListener));

        private ICodec Codec { get; }

        private ILogger Logger { get; }

        private ClientSettings ClientSettings { get; }

        public ClientFactory(ICodec codec, ILogger<ClientFactory> logger, UraganoSettings uraganoSettings)
        {
            Codec = codec;
            Logger = logger;
            ClientSettings = uraganoSettings.ClientSettings;
        }

        public async Task RemoveClient(string host, int port)
        {
            if (!_clients.TryRemove((host, port), out var client)) return;
            await client.Result.DisconnectAsync();
        }

        public async Task RemoveAllClient()
        {
            foreach (var (host, port) in _clients.Keys)
            {
                await RemoveClient(host, port);
            }
        }

        public async Task<IClient> CreateClientAsync(string serviceName, ServiceNodeInfo nodeInfo)
        {
            var key = (nodeInfo.Address, nodeInfo.Port);
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
                            if (nodeInfo.EnableTls)
                            {

                                var cert = ClientSettings?.ServicesCert?.FirstOrDefault(p => p.Key == serviceName).Value ?? ClientSettings?.DefaultCert;
                                if (cert == null)
                                {
                                    Logger.LogCritical($"Service {serviceName}[{nodeInfo.Address}:{nodeInfo.Port}] has TLS enabled, please configure the certificate.");
                                    throw new InvalidOperationException(
                                        $"Service {serviceName}[{nodeInfo.Address}:{nodeInfo.Port}] has TLS enabled, please configure the certificate.");
                                }

                                var targetHost = cert.Cert.GetNameInfo(X509NameType.DnsName, false);
                                pipeline.AddLast(new TlsHandler(stream =>
                                {
                                    return new SslStream(stream, true,
                                        (sender, certificate, chain, errors) =>
                                        {
                                            var success = SslPolicyErrors.None == errors;
                                            Logger.LogError(
                                                "The remote certificate is invalid according to the validation procedure:{0}.",
                                                errors);
                                            return success;
                                        });
                                }, new ClientTlsSettings(targetHost)));
                            }

                            //pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                            pipeline.AddLast(new LengthFieldPrepender(4));
                            pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                            pipeline.AddLast(new MessageDecoder<IServiceResult>(Codec));
                            pipeline.AddLast(new MessageEncoder<IInvokeMessage>(Codec));
                            pipeline.AddLast(new ClientMessageHandler(this, Logger));
                        }));

                    EndPoint endPoint;
                    if (IPAddress.TryParse(nodeInfo.Address, out var ip))
                        endPoint = new IPEndPoint(ip, nodeInfo.Port);
                    else
                        endPoint = new DnsEndPoint(nodeInfo.Address, nodeInfo.Port);
                    var channel = await bootstrap.ConnectAsync(endPoint);
                    channel.GetAttribute(TransportContextAttributeKey).Set(new TransportContext
                    {
                        Host = nodeInfo.Address,
                        Port = nodeInfo.Port
                    });

                    var listener = new MessageListener();
                    channel.GetAttribute(MessageListenerAttributeKey).Set(listener);
                    return new Client(channel, group, listener, Logger, Codec, $"{nodeInfo.Address}:{ nodeInfo.Port}");
                });
            }
            catch
            {
                _clients.TryRemove(key, out _);
                throw;
            }
        }

        internal class ClientMessageHandler : ChannelHandlerAdapter
        {

            private IClientFactory ClientFactory { get; }

            private ILogger Logger { get; }

            public ClientMessageHandler(IClientFactory clientFactory, ILogger logger)
            {
                ClientFactory = clientFactory;
                Logger = logger;
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var msg = message as TransportMessage<IServiceResult>;
                var listener = context.Channel.GetAttribute(MessageListenerAttributeKey).Get();
                listener.Received(msg);
            }

            public override void ChannelInactive(IChannelHandlerContext context)
            {
                Logger.LogCritical("The status of client {0} is unavailable,Please check the network and certificate!", context.Channel.RemoteAddress);
                var ctx = context.Channel.GetAttribute(TransportContextAttributeKey).Get();
                ClientFactory.RemoveClient(ctx.Host, ctx.Port).GetAwaiter().GetResult();
                base.ChannelInactive(context);
            }
        }
    }
}
