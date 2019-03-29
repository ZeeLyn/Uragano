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

        private ServerSettings ServerSettings { get; }

        public ServerMessageHandler(IServiceFactory serviceFactory, IServiceProvider serviceProvider, ILogger logger, ICodec codec, ServerSettings serverSettings)
        {
            ServiceFactory = serviceFactory;
            ServiceProvider = serviceProvider;
            Logger = logger;
            Codec = codec;
            ServerSettings = serverSettings;
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
                        Logger.LogTrace($"\nThe server received the message:\nCurrent node:{ServerSettings}\nRoute:{transportMessage.Body.Route}\nMessage id:{transportMessage.Id}\nArgs:{Codec.ToJson(transportMessage.Body.Args)}\nMeta:{Codec.ToJson(transportMessage.Body.Meta)}\n\n");
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
                    Logger.LogError(e, $"\nThe server message processing failed:{e.Message}.\nCurrent node:{ServerSettings}\nRoute:{transportMessage.Body.Route}\nMessage id:{transportMessage.Id}\n\n");
                    await context.WriteAndFlushAsync(new TransportMessage<IServiceResult>
                    {
                        Id = transportMessage.Id,
                        Body = new ServiceResult(e.Message, RemotingStatus.NotFound)
                    });
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"\nThe server message processing failed:{e.Message}.\nCurrent node:{ServerSettings}\nRoute:{transportMessage.Body.Route}\nMessage id:{transportMessage.Id}\n\n");
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
