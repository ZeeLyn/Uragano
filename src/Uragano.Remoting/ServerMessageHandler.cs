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

        public ServerMessageHandler(IServiceFactory serviceFactory, IServiceProvider serviceProvider, ILogger logger)
        {
            ServiceFactory = serviceFactory;
            ServiceProvider = serviceProvider;
            Logger = logger;
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
                    if (Logger.IsEnabled(LogLevel.Debug))
                        Logger.LogTrace($"Received the message:[route:{transportMessage.Body.Route};message id:{transportMessage.Id}]");
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
                    Logger.LogError(e, $"Message processing failed:{e.Message}.[route:{transportMessage.Body.Route};message id:{transportMessage.Id}]");
                    await context.WriteAndFlushAsync(new TransportMessage<IServiceResult>
                    {
                        Id = transportMessage.Id,
                        Body = new ServiceResult(e.Message, RemotingStatus.NotFound)
                    });
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"Message processing failed:{e.Message}.[route:{transportMessage.Body.Route};message id:{transportMessage.Id}]");
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
