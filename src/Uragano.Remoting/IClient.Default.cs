using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Uragano.Abstractions;

namespace Uragano.Remoting
{
    public class Client : IClient
    {
        private IChannel Channel { get; }
        private readonly ConcurrentDictionary<string, TaskCompletionSource<IServiceResult>> _resultCallbackTask =
            new ConcurrentDictionary<string, TaskCompletionSource<IServiceResult>>();

        private IMessageListener MessageListener { get; }

        public Client(IChannel channel, IMessageListener messageListener)
        {
            Channel = channel;
            MessageListener = messageListener;
            MessageListener.OnReceived += MessageListener_OnReceived;

        }

        private void MessageListener_OnReceived(TransportMessage<IServiceResult> message)
        {
            if (_resultCallbackTask.TryGetValue(message.Id, out var task))
            {
                task.TrySetResult(message.Body);
            }
            else
                Console.WriteLine("Not found callback");
        }

        public async Task<IServiceResult> SendAsync(IInvokeMessage message)
        {
            var transportMessage = new TransportMessage<IInvokeMessage>
            {
                Id = Guid.NewGuid().ToString(),
                Body = message
            };

            var tcs = new TaskCompletionSource<IServiceResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (var ct = new CancellationTokenSource(UraganoOptions.Remoting_Invoke_CancellationTokenSource_Timeout.Value))
            {
                ct.Token.Register(() => { tcs.TrySetResult(new ServiceResult("Remoting invoke timeout!", RemotingStatus.Timeout)); }, false);
                if (!_resultCallbackTask.TryAdd(transportMessage.Id, tcs)) throw new Exception("Failed to send.");
                try
                {
                    await Channel.WriteAndFlushAsync(transportMessage);
                    return await tcs.Task;
                }
                finally
                {
                    _resultCallbackTask.TryRemove(transportMessage.Id, out var t);
                    t.TrySetCanceled();
                }
            }
        }


        public void Dispose()
        {
            foreach (var task in _resultCallbackTask.Values)
            {
                task.TrySetCanceled();
            }

            Channel.DisconnectAsync();
            Channel.CloseAsync();
        }
    }
}
