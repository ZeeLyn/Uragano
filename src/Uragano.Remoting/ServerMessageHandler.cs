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

        private IServiceFactory InvokerFactory { get; }

        private IServiceProvider ServiceProvider { get; }

        private ILogger Logger { get; }

        public ServerMessageHandler(IServiceFactory invokerFactory, IServiceProvider serviceProvider, ILogger logger)
        {
            InvokerFactory = invokerFactory;
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
                    Logger.LogDebug($"Invoke route[{transportMessage.Body.Route}]");
                    var result = await InvokerFactory.Invoke(transportMessage.Body.Route, transportMessage.Body.Args,
                        transportMessage.Body.Meta);
                    await context.WriteAndFlushAsync(new TransportMessage<IServiceResult>
                    {
                        Id = transportMessage.Id,
                        Body = result
                    });
                }
                catch (NotFoundRouteException e)
                {
                    Logger.LogError(e, e.Message);
                    await context.WriteAndFlushAsync(new TransportMessage<IServiceResult>
                    {
                        Id = transportMessage.Id,
                        Body = new ServiceResult(e.Message, RemotingStatus.NotFound)
                    });
                }
                catch (Exception e)
                {
                    Logger.LogError(e, e.Message);
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
