using System;
using System.Net;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.Abstractions.Service;


namespace Uragano.Remoting
{
    public class ServerBootstrap : IBootstrap
    {
        private IChannel Channel { get; set; }


        private IServiceFactory ServiceFactory { get; }

        private ServerSettings ServerSettings { get; }

        private IServiceProvider ServiceProvider { get; }

        private ILogger Logger { get; }

        private ICodec Codec { get; }

        public ServerBootstrap(IServiceFactory serviceFactory, IServiceProvider serviceProvider, UraganoSettings uraganoSettings, ILogger<ServerBootstrap> logger, ICodec codec)
        {

            ServiceFactory = serviceFactory;
            ServiceProvider = serviceProvider;
            ServerSettings = uraganoSettings.ServerSettings;
            Logger = logger;
            Codec = codec;
        }

        private IEventLoopGroup BossGroup { get; set; }
        private IEventLoopGroup WorkerGroup { get; set; }

        public async Task StartAsync()
        {
            try
            {
                var bootstrap = new DotNetty.Transport.Bootstrapping.ServerBootstrap();
                if (UraganoOptions.DotNetty_Enable_Libuv.Value)
                {
                    var dispatcher = new DispatcherEventLoopGroup();
                    BossGroup = dispatcher;
                    WorkerGroup = new WorkerEventLoopGroup(dispatcher);
                    bootstrap.Channel<TcpServerChannel>();
                }
                else
                {
                    BossGroup = new MultithreadEventLoopGroup(1);
                    WorkerGroup = new MultithreadEventLoopGroup(UraganoOptions.DotNetty_Event_Loop_Count.Value);
                    bootstrap.Channel<TcpServerSocketChannel>();
                }

                bootstrap
                    .Group(BossGroup, WorkerGroup)
                    .Option(ChannelOption.SoBacklog, UraganoOptions.Server_DotNetty_Channel_SoBacklog.Value)
                    .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .ChildOption(ChannelOption.ConnectTimeout, UraganoOptions.DotNetty_Connect_Timeout.Value)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        var pipeline = channel.Pipeline;
                        if (ServerSettings.X509Certificate2 != null)
                        {
                            pipeline.AddLast(TlsHandler.Server(ServerSettings.X509Certificate2));
                        }

                        //pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                        pipeline.AddLast(new LengthFieldPrepender(4));
                        pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                        pipeline.AddLast(new MessageDecoder<IInvokeMessage>(Codec));
                        pipeline.AddLast(new MessageEncoder<IServiceResult>(Codec));
                        pipeline.AddLast(new ServerMessageHandler(ServiceFactory, ServiceProvider, Logger));
                    }));

                EndPoint endPoint;
                if (IPAddress.TryParse(ServerSettings.Address, out var iPAddress))
                    endPoint = new IPEndPoint(iPAddress, ServerSettings.Port);
                else
                    endPoint = new DnsEndPoint(ServerSettings.Address, ServerSettings.Port);
                Channel = await bootstrap.BindAsync(endPoint);
                Logger.LogInformation($"Uragano server listening {ServerSettings}");
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Start server[{ServerSettings}] failed!");
            }
        }


        public async Task StopAsync()
        {
            Logger.LogInformation("Stopping uragano server...");
            await Channel.CloseAsync();
            await BossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            await WorkerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            Logger.LogInformation("The uragano server has stopped.");
        }
    }
}
