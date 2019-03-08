using System;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.Abstractions.Exceptions;
using Uragano.Abstractions.Service;

namespace Uragano.Remoting
{
    public class ServerMessageHandler : ChannelHandlerAdapter
    {

        private IServiceFactory ServiceFactory { get; }

        private IServiceProvider ServiceProvider { get; }

        private ILogger Logger { get; }

        private ICodec Codec { get; }

        public ServerMessageHandler(IServiceFactory serviceFactory, IServiceProvider serviceProvider, ILogger logger, ICodec codec)
        {
            ServiceFactory = serviceFactory;
            ServiceProvider = serviceProvider;
            Logger = logger;
            Codec = codec;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            Task.Run(async () =>
            {
                var msg = message;
                if (!(msg is TransportMessage<IInvokeMessage> transportMessage))
                    throw new ArgumentNullException(nameof(message));
                try
                {
                    if (Logger.IsEnabled(LogLevel.Trace))
                        Logger.LogTrace($"\nReceived the message:\nRoute:{transportMessage.Body.Route}\nMessage id:{transportMessage.Id}\nArgs:{Codec.ToJson(transportMessage.Body.Args)}\nMeta:{Codec.ToJson(transportMessage.Body.Meta)}");
                    var result = await ServiceFactory.InvokeAsync(transportMessage.Body.Route, transportMessage.Body.Args,
                        transportMessage.Body.Meta);
                    await context.WriteAndFlushAsync(new TransportMessage<IServiceResult>
                    {
                        Id = transportMessage.Id,
                        Body = result
                    });
                }
                catch (NotFoundRouteException e)
                {
                    Logger.LogError(e, $"\nMessage processing failed:{e.Message}.\nRoute:{transportMessage.Body.Route}\nMessage id:{transportMessage.Id}");
                    await context.WriteAndFlushAsync(new TransportMessage<IServiceResult>
                    {
                        Id = transportMessage.Id,
                        Body = new ServiceResult(e.Message, RemotingStatus.NotFound)
                    });
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"\nMessage processing failed:{e.Message}.\nRoute:{transportMessage.Body.Route}\nMessage id:{transportMessage.Id}");
                    await context.WriteAndFlushAsync(new TransportMessage<IServiceResult>
                    {
                        Id = transportMessage.Id,
                        Body = new ServiceResult(e.Message, RemotingStatus.Error)
                    });
                }

            });
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.CloseAsync();
        }
    }
}
